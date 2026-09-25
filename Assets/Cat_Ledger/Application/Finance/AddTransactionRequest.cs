using System;
using CatLedger.Domain.Finance;

namespace CatLedger.Application.Finance
{
    /// <summary>
    /// 輸入收支請求
    /// </summary>
    public sealed class AddTransactionRequest
    {
        public long AmountInMinorUnits { get; }

        public TransactionType Type { get; }

        public TransactionCategory Category { get; }

        public PaymentMethod PaymentMethod { get; }

        public DateTimeOffset OccurredAt { get; }

        public string Note { get; }

        public AddTransactionRequest(
            long amountInMinorUnits,
            TransactionType type,
            TransactionCategory category,
            PaymentMethod paymentMethod,
            DateTimeOffset occurredAt,
            string note)
        {
            AmountInMinorUnits = amountInMinorUnits;
            Type = type;
            Category = category;
            PaymentMethod = paymentMethod;
            OccurredAt = occurredAt;
            Note = note;
        }
    }
}