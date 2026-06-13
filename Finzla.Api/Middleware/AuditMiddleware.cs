using Finzla.Application.Features;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;

namespace Finzla.Api.Middleware
{
    public sealed class AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        private static readonly string[] SkipPaths =
            ["/swagger", "/health", "/favicon.ico"];

        public async Task InvokeAsync(HttpContext context)
        {
            if (SkipPaths.Any(p => context.Request.Path.StartsWithSegments(p)))
            {
                await next(context);
                return;
            }

            var sw = Stopwatch.StartNew();

            context.Request.EnableBuffering();
            string? requestBody = null;
            if (context.Request.ContentLength > 0 && context.Request.ContentLength < 10_000)
            {
                requestBody = await new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true)
                    .ReadToEndAsync();
                context.Request.Body.Position = 0;
                if (requestBody.Contains("password", StringComparison.OrdinalIgnoreCase))
                    requestBody = "[REDACTED]";
            }

            await next(context);
            sw.Stop();

            var username = context.User?.FindFirstValue(ClaimTypes.Name) ?? "anonymous";
            var path     = context.Request.Path.Value ?? "/";
            var method   = context.Request.Method;
            var status   = context.Response.StatusCode;
            var action   = DeriveAction(method, path);
            var ip       = context.Connection.RemoteIpAddress?.ToString();
            var ua       = context.Request.Headers.UserAgent.FirstOrDefault();

            string? errorMsg = status >= 500 ? $"HTTP {status} error" : null;

            // Fire-and-forget inside a scope (don't block the response)
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = context.RequestServices.CreateScope();
                    var auditService = scope.ServiceProvider.GetRequiredService<AuditLogService>();
                    await auditService.RecordAsync(
                        username, action, path, method, status,
                        ip, ua, requestBody, sw.ElapsedMilliseconds, errorMsg);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Audit background write failed");
                }
            });
        }

        private static string DeriveAction(string method, string path) =>
            (method.ToUpperInvariant(), path.ToLowerInvariant()) switch
            {
                ("POST",   var p) when p.Contains("/auth/login")            => "LOGIN",
                ("POST",   var p) when p.Contains("/webhooks/transactions") => "INGEST_TRANSACTION",
                ("GET",    var p) when p.Contains("/accounts")              => "GET_ACCOUNT_SUMMARY",
                ("POST",   var p) when p.Contains("/users")                 => "CREATE_USER",
                ("GET",    var p) when p.Contains("/users")                 => "GET_USERS",
                ("GET",    var p) when p.Contains("/audit")                 => "GET_AUDIT_LOGS",
                ("PATCH",  var p) when p.Contains("/users")                 => "UPDATE_USER",
                _ => $"{method}_{path.Trim('/').Replace('/', '_').ToUpperInvariant()}"
            };
    }
}
