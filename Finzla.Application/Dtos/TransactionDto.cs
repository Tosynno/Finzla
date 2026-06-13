using System;
using System.Collections.Generic;
using System.Text;

namespace Finzla.Application.Dtos
{
    public sealed record IngestTransactionRequest(
     string TraceId,
     string DebitAccount,
     string AccountId,
     string Currency,
     decimal Amount,
     string Type,           
     DateTime OccurredAt,
     bool IsOtherBank = false
 );

    public sealed record IngestTransactionResponse(
        string DebitAccount,
        string Status,         
        AccountSummaryDto AccountSummary
    );

    public sealed record AccountSummaryDto(
        string AccountId,
        decimal Balance,
        decimal TotalCredits,
        decimal TotalDebits,
        int TotalTransactions,
        DateTime LastUpdatedAt
    );

}
