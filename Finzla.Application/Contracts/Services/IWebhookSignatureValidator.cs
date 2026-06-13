using System;
using System.Collections.Generic;
using System.Text;

namespace Finzla.Application.Contracts.Services
{
    public interface IWebhookSignatureValidator
    {
        bool IsValid(string payload, string incomingSignature);
    }
}
