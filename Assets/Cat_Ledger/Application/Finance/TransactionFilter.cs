using System;
using CatLedger.Domain.Finance;

namespace CatLedger.Application.Finance
{
    public sealed class TransactionFilter
    {
        public DateTimeOffset? StartOccurredAtInclusive { get; }

        public DateTimeOffset? EndOccurredAtExclusive { get; }

        public TransactionCategory? Category { get; }

        public TransactionType? Type { get; }

        public TransactionFilter(
            DateTimeOffset? startOccurredAtInclusive = null,
            DateTimeOffset? endOccurredAtExclusive = null,
            TransactionCategory? category = null,
            TransactionType? type = null)
        {
            if (startOccurredAtInclusive.HasValue &&
                endOccurredAtExclusive.HasValue &&
                startOccurredAtInclusive.Value >= endOccurredAtExclusive.Value)
            {
                throw new ArgumentException(
                    "The start time must be earlier than the end time.");
            }

            StartOccurredAtInclusive = startOccurredAtInclusive;
            EndOccurredAtExclusive = endOccurredAtExclusive;
            Category = category;
            Type = type;
        }
    }
}