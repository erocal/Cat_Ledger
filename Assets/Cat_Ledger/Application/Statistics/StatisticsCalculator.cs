using System;
using System.Collections.Generic;
using CatLedger.Domain.Finance;

namespace CatLedger.Application.Statistics
{
    public sealed class StatisticsCalculator
    {
        public StatisticsReport Calculate(
            IReadOnlyList<Transaction> transactions,
            DateTimeOffset startOccurredAtInclusive,
            DateTimeOffset endOccurredAtExclusive,
            TimeZoneInfo reportTimeZone)
        {
            if (transactions == null)
            {
                throw new ArgumentNullException(nameof(transactions));
            }

            if (reportTimeZone == null)
            {
                throw new ArgumentNullException(nameof(reportTimeZone));
            }

            if (endOccurredAtExclusive <= startOccurredAtInclusive)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(endOccurredAtExclusive),
                    "End time must be after start time.");
            }

            long totalIncomeInMinorUnits = 0;
            long totalExpenseInMinorUnits = 0;

            var dailyTotalsByDate =
                new Dictionary<DateTime, DailyAccumulator>();

            var expenseTotalsByCategory =
                new Dictionary<TransactionCategory, long>();

            for (int index = 0; index < transactions.Count; index++)
            {
                Transaction transaction = transactions[index];

                if (transaction == null)
                {
                    throw new ArgumentException(
                        $"Transaction at index {index} is null.",
                        nameof(transactions));
                }

                if (transaction.OccurredAt < startOccurredAtInclusive ||
                    transaction.OccurredAt >= endOccurredAtExclusive)
                {
                    continue;
                }

                long amountInMinorUnits =
                    transaction.AmountInMinorUnits;

                if (amountInMinorUnits < 0)
                {
                    throw new ArgumentException(
                        "Transaction amount cannot be negative.",
                        nameof(transactions));
                }

                DateTime reportDate = TimeZoneInfo.ConvertTime(
                    transaction.OccurredAt,
                    reportTimeZone).Date;

                if (!dailyTotalsByDate.TryGetValue(
                    reportDate,
                    out DailyAccumulator dailyTotals))
                {
                    dailyTotals = new DailyAccumulator();
                    dailyTotalsByDate.Add(reportDate, dailyTotals);
                }

                switch (transaction.Type)
                {
                    case TransactionType.Income:
                        totalIncomeInMinorUnits = checked(
                            totalIncomeInMinorUnits + amountInMinorUnits);

                        dailyTotals.TotalIncomeInMinorUnits = checked(
                            dailyTotals.TotalIncomeInMinorUnits +
                            amountInMinorUnits);
                        break;

                    case TransactionType.Expense:
                        totalExpenseInMinorUnits = checked(
                            totalExpenseInMinorUnits + amountInMinorUnits);

                        dailyTotals.TotalExpenseInMinorUnits = checked(
                            dailyTotals.TotalExpenseInMinorUnits +
                            amountInMinorUnits);

                        expenseTotalsByCategory.TryGetValue(
                            transaction.Category,
                            out long categoryExpenseInMinorUnits);

                        expenseTotalsByCategory[transaction.Category] = checked(
                            categoryExpenseInMinorUnits + amountInMinorUnits);
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Unsupported transaction type: {transaction.Type}");
                }
            }

            List<DailyStatistics> dailyStatistics =
                CreateDailyStatistics(dailyTotalsByDate);

            List<CategoryStatistics> categoryStatistics =
                CreateCategoryStatistics(
                    expenseTotalsByCategory,
                    totalExpenseInMinorUnits);

            return new StatisticsReport(
                startOccurredAtInclusive,
                endOccurredAtExclusive,
                totalIncomeInMinorUnits,
                totalExpenseInMinorUnits,
                dailyStatistics,
                categoryStatistics);
        }

        private static List<DailyStatistics> CreateDailyStatistics(
            IReadOnlyDictionary<DateTime, DailyAccumulator> totalsByDate)
        {
            var results = new List<DailyStatistics>(totalsByDate.Count);

            foreach (KeyValuePair<DateTime, DailyAccumulator> entry
                     in totalsByDate)
            {
                results.Add(new DailyStatistics(
                    entry.Key,
                    entry.Value.TotalIncomeInMinorUnits,
                    entry.Value.TotalExpenseInMinorUnits));
            }

            results.Sort(
                (left, right) => left.Date.CompareTo(right.Date));

            return results;
        }

        private static List<CategoryStatistics> CreateCategoryStatistics(
            IReadOnlyDictionary<TransactionCategory, long> totalsByCategory,
            long totalExpenseInMinorUnits)
        {
            var results =
                new List<CategoryStatistics>(totalsByCategory.Count);

            foreach (KeyValuePair<TransactionCategory, long> entry
                     in totalsByCategory)
            {
                if (entry.Value == 0)
                {
                    continue;
                }

                decimal expenseRatio =
                    totalExpenseInMinorUnits == 0
                        ? 0m
                        : (decimal)entry.Value / totalExpenseInMinorUnits;

                results.Add(new CategoryStatistics(
                    entry.Key,
                    entry.Value,
                    expenseRatio));
            }

            results.Sort((left, right) =>
            {
                int amountComparison =
                    right.TotalExpenseInMinorUnits.CompareTo(
                        left.TotalExpenseInMinorUnits);

                return amountComparison != 0
                    ? amountComparison
                    : left.Category.CompareTo(right.Category);
            });

            return results;
        }

        private sealed class DailyAccumulator
        {
            public long TotalIncomeInMinorUnits { get; set; }

            public long TotalExpenseInMinorUnits { get; set; }
        }
    }
}
