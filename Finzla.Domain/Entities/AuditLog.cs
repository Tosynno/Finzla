namespace Finzla.Domain.Entities
{
    public sealed class AuditLog
    {
        private AuditLog() { }

        public long Id { get; private set; }
        public string Username { get; private set; } = default!;
        public string Action { get; private set; } = default!;       // e.g. "LOGIN", "INGEST_TRANSACTION"
        public string Resource { get; private set; } = default!;     // e.g. "/api/auth/login"
        public string HttpMethod { get; private set; } = default!;
        public int StatusCode { get; private set; }
        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }
        public string? RequestBody { get; private set; }
        public string? ErrorMessage { get; private set; }
        public long DurationMs { get; private set; }
        public DateTime OccurredAt { get; private set; }

        public static AuditLog Create(
            string username,
            string action,
            string resource,
            string httpMethod,
            int statusCode,
            string? ipAddress,
            string? userAgent,
            string? requestBody,
            long durationMs,
            string? errorMessage = null) =>
            new()
            {
                Username    = username,
                Action      = action,
                Resource    = resource,
                HttpMethod  = httpMethod,
                StatusCode  = statusCode,
                IpAddress   = ipAddress,
                UserAgent   = userAgent,
                RequestBody = requestBody,
                DurationMs  = durationMs,
                ErrorMessage = errorMessage,
                OccurredAt  = DateTime.UtcNow
            };
    }
}
