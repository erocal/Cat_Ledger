using System;
using CatLedger.Application.Statistics;
using CatLedger.Domain.Finance;
using NUnit.Framework;

namespace CatLedger.Tests.EditMode.Statistics
{
    public sealed class StatisticsCalculatorTests
    {
        private static readonly TimeZoneInfo ReportTimeZone =
            TimeZoneInfo.CreateCustomTimeZone(
                "CatLedger.Test.UTC+09",
                TimeSpan.FromHours(9),
                "UTC+09 Test",
                "UTC+09 Test");

        private static readonly DateTimeOffset NovemberStart =
            new DateTimeOffset(
                2026, 11, 1, 0, 0, 0,
                TimeSpan.FromHours(9));

        private static readonly DateTimeOffset DecemberStart =
            new DateTimeOffset(
                2026, 12, 1, 0, 0, 0,
                TimeSpan.FromHours(9));

        private readonly StatisticsCalculator _calculator =
            new StatisticsCalculator();

        [Test]
        public void Calculate_WithNoTransactions_ReturnsEmptyReport()
        {
            StatisticsReport report = _calculator.Calculate(
                Array.Empty<Transaction>(),
                NovemberStart,
                DecemberStart,
                ReportTimeZone);

            Assert.That(report.TotalIncomeInMinorUnits, Is.Zero);
            Assert.That(report.TotalExpenseInMinorUnits, Is.Zero);
            Assert.That(report.BalanceInMinorUnits, Is.Zero);
            Assert.That(report.DailyStatistics, Is.Empty);
            Assert.That(report.CategoryStatistics, Is.Empty);
        }

        [Test]
        public void Calculate_WithMixedTransactions_AggregatesCorrectly()
        {
            Transaction[] transactions =
            {
                CreateTransaction(
                    10000,
                    TransactionType.Income,
                    TransactionCategory.Other,
                    NovemberStart.AddHours(9)),

                CreateTransaction(
                    400,
                    TransactionType.Expense,
                    TransactionCategory.Transport,
                    NovemberStart.AddDays(1).AddHours(14)),

                CreateTransaction(
                    600,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    NovemberStart.AddHours(12))
            };

            StatisticsReport report = _calculator.Calculate(
                transactions,
                NovemberStart,
                DecemberStart,
                ReportTimeZone);

            Assert.That(report.TotalIncomeInMinorUnits,
                Is.EqualTo(10000));

            Assert.That(report.TotalExpenseInMinorUnits,
                Is.EqualTo(1000));

            Assert.That(report.BalanceInMinorUnits,
                Is.EqualTo(9000));

            Assert.That(report.DailyStatistics.Count,
                Is.EqualTo(2));

            Assert.That(report.DailyStatistics[0].Date,
                Is.EqualTo(new DateTime(2026, 11, 1)));

            Assert.That(
                report.DailyStatistics[0].TotalIncomeInMinorUnits,
                Is.EqualTo(10000));

            Assert.That(
                report.DailyStatistics[0].TotalExpenseInMinorUnits,
                Is.EqualTo(600));

            Assert.That(report.DailyStatistics[1].Date,
                Is.EqualTo(new DateTime(2026, 11, 2)));

            Assert.That(
                report.DailyStatistics[1].TotalExpenseInMinorUnits,
                Is.EqualTo(400));

            Assert.That(report.CategoryStatistics.Count,
                Is.EqualTo(2));

            Assert.That(report.CategoryStatistics[0].Category,
                Is.EqualTo(TransactionCategory.Food));

            Assert.That(
                report.CategoryStatistics[0].TotalExpenseInMinorUnits,
                Is.EqualTo(600));

            Assert.That(report.CategoryStatistics[0].ExpenseRatio,
                Is.EqualTo(0.6m));

            Assert.That(report.CategoryStatistics[1].Category,
                Is.EqualTo(TransactionCategory.Transport));

            Assert.That(report.CategoryStatistics[1].ExpenseRatio,
                Is.EqualTo(0.4m));
        }

        [Test]
        public void Calculate_WhenExpenseExceedsIncome_ReturnsNegativeBalance()
        {
            Transaction[] transactions =
            {
                CreateTransaction(
                    500,
                    TransactionType.Income,
                    TransactionCategory.Other,
                    NovemberStart.AddHours(9)),

                CreateTransaction(
                    900,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    NovemberStart.AddHours(12))
            };

            StatisticsReport report = _calculator.Calculate(
                transactions,
                NovemberStart,
                DecemberStart,
                ReportTimeZone);

            Assert.That(report.BalanceInMinorUnits,
                Is.EqualTo(-400));

            Assert.That(
                report.DailyStatistics[0].BalanceInMinorUnits,
                Is.EqualTo(-400));
        }

        [Test]
        public void Calculate_AtDateBoundaries_FiltersCorrectly()
        {
            Transaction[] transactions =
            {
                CreateTransaction(
                    50,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    NovemberStart.AddTicks(-1)),

                CreateTransaction(
                    100,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    NovemberStart),

                CreateTransaction(
                    300,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    DecemberStart.AddTicks(-1)),

                CreateTransaction(
                    900,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    DecemberStart)
            };

            StatisticsReport report = _calculator.Calculate(
                transactions,
                NovemberStart,
                DecemberStart,
                ReportTimeZone);

            Assert.That(report.TotalExpenseInMinorUnits,
                Is.EqualTo(400));

            Assert.That(report.DailyStatistics.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void Calculate_UsingReportTimeZone_GroupsByLocalDate()
        {
            DateTimeOffset decemberFirst =
                new DateTimeOffset(
                    2026, 12, 1, 0, 0, 0,
                    TimeSpan.FromHours(9));

            DateTimeOffset decemberThird =
                new DateTimeOffset(
                    2026, 12, 3, 0, 0, 0,
                    TimeSpan.FromHours(9));

            Transaction[] transactions =
            {
                // UTC Nov 30 15:30 -> JST Dec 1 00:30
                CreateTransaction(
                    100,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    new DateTimeOffset(
                        2026, 11, 30, 15, 30, 0,
                        TimeSpan.Zero)),

                // UTC Dec 1 15:05 -> JST Dec 2 00:05
                CreateTransaction(
                    200,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    new DateTimeOffset(
                        2026, 12, 1, 15, 5, 0,
                        TimeSpan.Zero))
            };

            StatisticsReport report = _calculator.Calculate(
                transactions,
                decemberFirst,
                decemberThird,
                ReportTimeZone);

            Assert.That(report.DailyStatistics.Count,
                Is.EqualTo(2));

            Assert.That(report.DailyStatistics[0].Date,
                Is.EqualTo(new DateTime(2026, 12, 1)));

            Assert.That(report.DailyStatistics[1].Date,
                Is.EqualTo(new DateTime(2026, 12, 2)));

            Assert.That(report.DailyStatistics[0].Date.Kind,
                Is.EqualTo(DateTimeKind.Unspecified));

            Assert.That(report.TotalExpenseInMinorUnits,
                Is.EqualTo(300));
        }

        [Test]
        public void Calculate_WithOnlyIncome_ReturnsNoExpenseCategories()
        {
            Transaction[] transactions =
            {
                CreateTransaction(
                    5000,
                    TransactionType.Income,
                    TransactionCategory.Other,
                    NovemberStart.AddHours(10))
            };

            StatisticsReport report = _calculator.Calculate(
                transactions,
                NovemberStart,
                DecemberStart,
                ReportTimeZone);

            Assert.That(report.TotalIncomeInMinorUnits,
                Is.EqualTo(5000));

            Assert.That(report.TotalExpenseInMinorUnits, Is.Zero);
            Assert.That(report.CategoryStatistics, Is.Empty);
        }

        [Test]
        public void Calculate_WithInvalidDateRange_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _calculator.Calculate(
                    Array.Empty<Transaction>(),
                    DecemberStart,
                    NovemberStart,
                    ReportTimeZone);
            });
        }

        [Test]
        public void Calculate_WithNullTransactions_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _calculator.Calculate(
                    null,
                    NovemberStart,
                    DecemberStart,
                    ReportTimeZone);
            });
        }

        [Test]
        public void Calculate_WithNullTimeZone_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _calculator.Calculate(
                    Array.Empty<Transaction>(),
                    NovemberStart,
                    DecemberStart,
                    null);
            });
        }

        [Test]
        public void Calculate_WithNullTransactionEntry_ThrowsException()
        {
            Transaction[] transactions = { null };

            Assert.Throws<ArgumentException>(() =>
            {
                _calculator.Calculate(
                    transactions,
                    NovemberStart,
                    DecemberStart,
                    ReportTimeZone);
            });
        }

        [Test]
        public void Calculate_WhenIncomeOverflows_ThrowsException()
        {
            Transaction[] transactions =
            {
                CreateTransaction(
                    long.MaxValue,
                    TransactionType.Income,
                    TransactionCategory.Other,
                    NovemberStart.AddHours(9)),

                CreateTransaction(
                    1,
                    TransactionType.Income,
                    TransactionCategory.Other,
                    NovemberStart.AddHours(10))
            };

            Assert.Throws<OverflowException>(() =>
            {
                _calculator.Calculate(
                    transactions,
                    NovemberStart,
                    DecemberStart,
                    ReportTimeZone);
            });
        }

        private static Transaction CreateTransaction(
            long amountInMinorUnits,
            TransactionType type,
            TransactionCategory category,
            DateTimeOffset occurredAt)
        {
            return new Transaction(
                Guid.NewGuid(),
                amountInMinorUnits: amountInMinorUnits,
                type: type,
                category: category,
                paymentMethod: PaymentMethod.Cash,
                occurredAt: occurredAt,
                note: "Statistics test transaction",
                createdAt: occurredAt,
                updatedAt: occurredAt);
        }
    }
}
