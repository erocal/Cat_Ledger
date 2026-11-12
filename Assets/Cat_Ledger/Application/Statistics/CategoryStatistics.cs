using System;
using CatLedger.Domain.Finance;

namespace CatLedger.Application.Statistics
{
    public sealed class CategoryStatistics
    {
        public TransactionCategory Category { get; }

        public long TotalExpenseInMinorUnits { get; }

        // Ratio from 0 to 1.
        // For example, 0.25 represents 25%.
        public decimal ExpenseRatio { get; }

        public CategoryStatistics(
            TransactionCategory category,
            long totalExpenseInMinorUnits,
            decimal expenseRatio)
        {
            if (totalExpenseInMinorUnits < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(totalExpenseInMinorUnits));
            }

            if (expenseRatio < 0m || expenseRatio > 1m)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(expenseRatio),
                    "Expense ratio must be between 0 and 1.");
            }

            Category = category;
            TotalExpenseInMinorUnits = totalExpenseInMinorUnits;
            ExpenseRatio = expenseRatio;
        }
    }
}
