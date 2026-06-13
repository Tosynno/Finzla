using Finzla.Application.Contracts.Services;
using System.Text;

namespace Finzla.Api.MIddleware
{
    public sealed class WebhookSignatureMiddleware(
        RequestDelegate next,
        IWebhookSignatureValidator validator,
        ILogger<WebhookSignatureMiddleware> logger)
    {
        private const string SignatureHeader     = "X-Webhook-Signature";
        private const string WebhookIngestPath   = "/api/webhooks/transactions";

        public async Task InvokeAsync(HttpContext context)
        {
            var isWebhookPost =
                context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
                context.Request.Path.StartsWithSegments(WebhookIngestPath);

            if (!isWebhookPost)
            {
                await next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(SignatureHeader, out var sig))
            {
                context.Request.EnableBuffering();
                await next(context);
                return;
            }

            context.Request.EnableBuffering();
            var body = await new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true)
                .ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!validator.IsValid(body, sig.ToString()))
            {
                logger.LogWarning("Webhook signature mismatch for {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid webhook signature." });
                return;
            }

            await next(context);
        }
    }
}
