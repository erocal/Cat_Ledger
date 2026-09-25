using System;

namespace CatLedger.Domain.Finance
{
    public sealed class Transaction
    {
        public Guid Id { get; }

        public long AmountInMinorUnits { get; private set; }

        public TransactionType Type { get; private set; }

        public TransactionCategory Category { get; private set; }

        public PaymentMethod PaymentMethod { get; private set; }

        public DateTimeOffset OccurredAt { get; private set; }

        public string Note { get; private set; }

        public DateTimeOffset CreatedAt { get; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public Transaction(
            Guid id,
            long amountInMinorUnits,
            TransactionType type,
            TransactionCategory category,
            PaymentMethod paymentMethod,
            DateTimeOffset occurredAt,
            string note,
            DateTimeOffset createdAt,
            DateTimeOffset updatedAt)
        {
            ValidateAmount(amountInMinorUnits);

            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Transaction ID cannot be empty.",
                    nameof(id));
            }

            Id = id;
            AmountInMinorUnits = amountInMinorUnits;
            Type = type;
            Category = category;
            PaymentMethod = paymentMethod;
            OccurredAt = occurredAt;
            Note = NormalizeNote(note);
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public void UpdateDetails(
            long amountInMinorUnits,
            TransactionType type,
            TransactionCategory category,
            PaymentMethod paymentMethod,
            DateTimeOffset occurredAt,
            string note,
            DateTimeOffset updatedAt)
        {
            ValidateAmount(amountInMinorUnits);

            AmountInMinorUnits = amountInMinorUnits;
            Type = type;
            Category = category;
            PaymentMethod = paymentMethod;
            OccurredAt = occurredAt;
            Note = NormalizeNote(note);
            UpdatedAt = updatedAt;
        }

        private static void ValidateAmount(long amountInMinorUnits)
        {
            if (amountInMinorUnits <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amountInMinorUnits),
                    "Transaction amount must be greater than zero.");
            }
        }

        private static string NormalizeNote(string note)
        {
            return string.IsNullOrWhiteSpace(note)
                ? string.Empty
                : note.Trim();
        }
    }
}