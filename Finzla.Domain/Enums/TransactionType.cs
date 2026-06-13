using System;
using System.Collections.Generic;
using System.Text;

namespace Finzla.Domain.Enums
{
    public enum TransactionType
    {
        Credit,
        Debit
    }

    public enum TransactionStatus
    {
        Accepted,
        Duplicate
    }
}
