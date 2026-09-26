using System;
using CatLedger.Domain.Finance;

namespace CatLedger.Tests.EditMode.Finance.TestData
{
    public static class TransactionTestFactory
    {
        public static Transaction Create(
            long amountInMinorUnits = 1000,
            TransactionType type = TransactionType.Expense,
            TransactionCategory category = TransactionCategory.Food,
            PaymentMethod paymentMethod = PaymentMethod.Cash,
            DateTimeOffset? occurredAt = null,
            string note = "")
        {
            DateTimeOffset transactionTime =
                occurredAt ??
                new DateTimeOffset(
                    2026,
                    9,
                    26,
                    12,
                    0,
                    0,
                    TimeSpan.FromHours(9));

            return new Transaction(
                Guid.NewGuid(),
                amountInMinorUnits,
                type,
                category,
                paymentMethod,
                transactionTime,
                note,
                transactionTime,
                transactionTime);
        }
    }
}