using System.Net;
using System.Text.Json;

namespace Finzla.Api.Middleware
{
    public sealed class GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        private static readonly JsonSerializerOptions JsonOpts =
            new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await WriteErrorAsync(context, ex);
            }
        }

        private static Task WriteErrorAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var (status, code, message) = ex switch
            {
                ArgumentNullException      => (HttpStatusCode.BadRequest,           "INVALID_INPUT",   ex.Message),
                InvalidOperationException  => (HttpStatusCode.UnprocessableEntity,  "BUSINESS_RULE",   ex.Message),
                UnauthorizedAccessException=> (HttpStatusCode.Unauthorized,         "UNAUTHORIZED",    "Access denied."),
                KeyNotFoundException       => (HttpStatusCode.NotFound,             "NOT_FOUND",       ex.Message),
                _                          => (HttpStatusCode.InternalServerError,  "SERVER_ERROR",    "An unexpected error occurred.")
            };

            context.Response.StatusCode = (int)status;

            var body = JsonSerializer.Serialize(
                new { error = code, detail = message, traceId = context.TraceIdentifier },
                JsonOpts);

            return context.Response.WriteAsync(body);
        }
    }
}
