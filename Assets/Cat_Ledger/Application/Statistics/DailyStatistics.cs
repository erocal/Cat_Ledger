using System;

namespace CatLedger.Application.Statistics
{
    public sealed class DailyStatistics
    {
        // Represents a calendar date in the report's time zone.
        // DateTimeKind is always Unspecified.
        public DateTime Date { get; }

        public long TotalIncomeInMinorUnits { get; }

        public long TotalExpenseInMinorUnits { get; }

        public long BalanceInMinorUnits { get; }

        public DailyStatistics(
            DateTime date,
            long totalIncomeInMinorUnits,
            long totalExpenseInMinorUnits)
        {
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

            Date = new DateTime(
                date.Year,
                date.Month,
                date.Day);

            TotalIncomeInMinorUnits = totalIncomeInMinorUnits;
            TotalExpenseInMinorUnits = totalExpenseInMinorUnits;

            BalanceInMinorUnits = checked(
                totalIncomeInMinorUnits - totalExpenseInMinorUnits);
        }
    }
}
