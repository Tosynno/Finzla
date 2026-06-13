using Finzla.Application.Contracts.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Finzla.Infrastructure.Services
{
    public sealed class HmacWebhookSignatureValidator(IConfiguration config) : IWebhookSignatureValidator
    {
        private readonly string _secret = config["Webhook:Secret"] ?? string.Empty;

        public bool IsValid(string payload, string incomingSignature)
        {
            if (string.IsNullOrWhiteSpace(_secret))
                return true;

            var keyBytes     = Encoding.UTF8.GetBytes(_secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            var computed     = Convert.ToHexString(HMACSHA256.HashData(keyBytes, payloadBytes))
                                      .ToLowerInvariant();
            var incoming     = incomingSignature.ToLowerInvariant();

            if (computed.Length != incoming.Length)
                return false;

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(incoming),
                Encoding.UTF8.GetBytes(computed));
        }
    }
}
