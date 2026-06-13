using Finzla.Application.Contracts.Services;
using Finzla.Infrastructure.Persistence;
using Finzla.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Finzla.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("Postgres"),
                    npgsql => npgsql.EnableRetryOnFailure(3)));

            // Repositories
            services.AddScoped<ITransactionRepository,    TransactionRepository>();
            services.AddScoped<IAccountSummaryRepository, AccountSummaryRepository>();
            services.AddScoped<IUserRepository,           UserRepository>();
            services.AddScoped<IAuditLogRepository,       AuditLogRepository>();

            // Services
            services.AddSingleton<IWebhookSignatureValidator, HmacWebhookSignatureValidator>();

            return services;
        }
    }
}
