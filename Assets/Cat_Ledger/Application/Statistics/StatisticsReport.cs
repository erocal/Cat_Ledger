using System;
using System.Collections.Generic;

namespace CatLedger.Application.Statistics
{
    public sealed class StatisticsReport
    {
        public DateTimeOffset StartOccurredAtInclusive { get; }

        public DateTimeOffset EndOccurredAtExclusive { get; }

        public long TotalIncomeInMinorUnits { get; }

        public long TotalExpenseInMinorUnits { get; }

        public long BalanceInMinorUnits { get; }

        public IReadOnlyList<DailyStatistics> DailyStatistics { get; }

        public IReadOnlyList<CategoryStatistics> CategoryStatistics { get; }

        public StatisticsReport(
            DateTimeOffset startOccurredAtInclusive,
            DateTimeOffset endOccurredAtExclusive,
            long totalIncomeInMinorUnits,
            long totalExpenseInMinorUnits,
            IReadOnlyList<DailyStatistics> dailyStatistics,
            IReadOnlyList<CategoryStatistics> categoryStatistics)
        {
            if (endOccurredAtExclusive <= startOccurredAtInclusive)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(endOccurredAtExclusive),
                    "The end time must be after the start time.");
            }

            if (totalIncomeInMinorUnits < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(totalIncomeInMinorUnits));
            }

            if (totalExpenseInMinorUnits < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(totalExpenseInMinorUnits));
            }

            if (dailyStatistics == null)
            {
                throw new ArgumentNullException(
                    nameof(dailyStatistics));
            }

            if (categoryStatistics == null)
            {
                throw new ArgumentNullException(
                    nameof(categoryStatistics));
            }

            StartOccurredAtInclusive = startOccurredAtInclusive;
            EndOccurredAtExclusive = endOccurredAtExclusive;

            TotalIncomeInMinorUnits = totalIncomeInMinorUnits;
            TotalExpenseInMinorUnits = totalExpenseInMinorUnits;

            BalanceInMinorUnits = checked(
                totalIncomeInMinorUnits - totalExpenseInMinorUnits);

            DailyStatistics =
                new List<DailyStatistics>(dailyStatistics).AsReadOnly();

            CategoryStatistics =
                new List<CategoryStatistics>(categoryStatistics).AsReadOnly();
        }
    }
}
