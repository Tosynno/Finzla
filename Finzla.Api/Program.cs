using Finzla.Api;
using Finzla.Api.Filters;
using Finzla.Api.Middleware;
using Finzla.Api.MIddleware;
using Finzla.Application;
using Finzla.Infrastructure;
using Finzla.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: $"logs/finzla-{DateTime.Now:yyyy-MM-dd}.txt",
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 15 * 1024 * 1024,
        rollOnFileSizeLimit: true,
        retainedFileCountLimit: 10)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

var jwtSecret = builder.Configuration["Auth:JwtSecret"]
    ?? throw new InvalidOperationException("Auth:JwtSecret is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Auth:Issuer"],
            ValidAudience            = builder.Configuration["Auth:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<FluentValidationFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<FluentValidationFilter>();
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Finzla Transaction API",
        Version     = "v1",
        Description = "Transaction ingestion, account summary, user management, and audit log API.\n\n" +
                      "**How to authenticate:**\n" +
                      "1. POST `/api/auth/login` with body `{username, password}` and header `AppSecurityKey`\n" +
                      "2. Copy the returned `token` value\n" +
                      "3. Click **Authorize** above, enter `Bearer <token>`"
    });

   
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token (without 'Bearer' prefix — Swagger adds it automatically)"
    });

    c.AddSecurityDefinition("AppSecurityKey", new OpenApiSecurityScheme
    {
        Name        = "AppSecurityKey",
        Type        = SecuritySchemeType.ApiKey,
        In          = ParameterLocation.Header,
        Description = "Required only for POST /api/auth/login"
    });

   
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db, builder.Configuration);
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Finzla API v1");
    c.RoutePrefix  = "swagger";
    c.DocumentTitle = "Finzla API";
    c.EnableDeepLinking();
});

app.UseMiddleware<GlobalExceptionMiddleware>();  
app.UseMiddleware<AuditMiddleware>();             
app.UseMiddleware<WebhookSignatureMiddleware>();  

app.UseHttpsRedirection();

app.UseAuthentication();  
app.UseAuthorization();   

app.MapControllers();

await app.RunAsync();
