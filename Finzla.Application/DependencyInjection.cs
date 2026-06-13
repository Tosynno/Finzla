using Finzla.Application.Features;
using Finzla.Application.Validators;
using FluentValidation;
//using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Finzla.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IngestTransactionService>();
            services.AddScoped<AccountSummaryService>();
            services.AddScoped<AuthService>();
            services.AddScoped<UserService>();
            services.AddScoped<AuditLogService>();

            // FluentValidation — auto-validates all requests before controllers are hit
            //services.AddFluentValidationAutoValidation(cfg =>
            //{
            //    cfg.DisableDataAnnotationsValidation = true;
            //});
            services.AddValidatorsFromAssemblyContaining<IngestTransactionRequestValidator>();

            return services;
        }
    }
}
