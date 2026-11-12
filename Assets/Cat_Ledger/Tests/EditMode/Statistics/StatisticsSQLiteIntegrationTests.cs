using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CatLedger.Application.Statistics;
using CatLedger.Domain.Finance;
using CatLedger.Infrastructure.Persistence.SQLite;
using NUnit.Framework;

namespace CatLedger.Tests.EditMode.Statistics
{
    public sealed class StatisticsSQLiteIntegrationTests
    {
        private static readonly TimeZoneInfo ReportTimeZone =
            TimeZoneInfo.CreateCustomTimeZone(
                "CatLedger.Test.UTC+09",
                TimeSpan.FromHours(9),
                "UTC+09 Test",
                "UTC+09 Test");

        // Test 1: Adding transactions updates the report.
        [Test]
        public async Task ExecuteMonthlyAsync_AfterAddingTransactions_AggregatesCorrectly()
        {
            await RunWithDatabaseAsync(async context =>
            {
                await context.Repository.AddAsync(CreateTransaction(
                    10000,
                    TransactionType.Income,
                    TransactionCategory.Other,
                    AtJapanTime(2026, 11, 1, 9, 0)));

                await context.Repository.AddAsync(CreateTransaction(
                    600,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    AtJapanTime(2026, 11, 1, 18, 0)));

                await context.Repository.AddAsync(CreateTransaction(
                    400,
                    TransactionType.Expense,
                    TransactionCategory.Transport,
                    AtJapanTime(2026, 11, 2, 9, 0)));

                await context.Repository.AddAsync(CreateTransaction(
                    300,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    AtJapanTime(2026, 11, 2, 21, 0)));

                StatisticsReport report =
                    await GetMonthlyReportAsync(context, 2026, 11);

                Assert.That(
                    report.TotalIncomeInMinorUnits,
                    Is.EqualTo(10000));

                Assert.That(
                    report.TotalExpenseInMinorUnits,
                    Is.EqualTo(1300));

                Assert.That(
                    report.BalanceInMinorUnits,
                    Is.EqualTo(8700));

                Assert.That(
                    report.DailyStatistics.Count,
                    Is.EqualTo(2));

                Assert.That(
                    report.DailyStatistics[0].Date,
                    Is.EqualTo(new DateTime(2026, 11, 1)));

                Assert.That(
                    report.DailyStatistics[0].TotalExpenseInMinorUnits,
                    Is.EqualTo(600));

                Assert.That(
                    report.DailyStatistics[1].TotalExpenseInMinorUnits,
                    Is.EqualTo(700));

                Assert.That(
                    report.CategoryStatistics.Count,
                    Is.EqualTo(2));

                Assert.That(
                    report.CategoryStatistics[0].Category,
                    Is.EqualTo(TransactionCategory.Food));

                Assert.That(
                    report.CategoryStatistics[0].TotalExpenseInMinorUnits,
                    Is.EqualTo(900));

                Assert.That(
                    report.CategoryStatistics[0].ExpenseRatio,
                    Is.EqualTo(900m / 1300m));

                Assert.That(
                    report.CategoryStatistics[1].Category,
                    Is.EqualTo(TransactionCategory.Transport));

                Assert.That(
                    report.CategoryStatistics[1].TotalExpenseInMinorUnits,
                    Is.EqualTo(400));
            });
        }

        // Test 2: Editing transaction details updates the report.
        [Test]
        public async Task ExecuteMonthlyAsync_AfterUpdatingTransaction_ReflectsChanges()
        {
            await RunWithDatabaseAsync(async context =>
            {
                Transaction transaction = CreateTransaction(
                    500,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    AtJapanTime(2026, 11, 10, 10, 0));

                await context.Repository.AddAsync(transaction);

                StatisticsReport beforeUpdate =
                    await GetMonthlyReportAsync(context, 2026, 11);

                Assert.That(
                    beforeUpdate.TotalExpenseInMinorUnits,
                    Is.EqualTo(500));

                transaction.UpdateDetails(
                    amountInMinorUnits: 1200,
                    type: TransactionType.Income,
                    category: TransactionCategory.Other,
                    paymentMethod: PaymentMethod.MobilePayment,
                    occurredAt: AtJapanTime(2026, 11, 18, 12, 0),
                    note: "Updated income",
                    updatedAt: AtJapanTime(2026, 11, 19, 12, 0));

                await context.Repository.UpdateAsync(transaction);

                StatisticsReport afterUpdate =
                    await GetMonthlyReportAsync(context, 2026, 11);

                Assert.That(
                    afterUpdate.TotalIncomeInMinorUnits,
                    Is.EqualTo(1200));

                Assert.That(
                    afterUpdate.TotalExpenseInMinorUnits,
                    Is.Zero);

                Assert.That(
                    afterUpdate.BalanceInMinorUnits,
                    Is.EqualTo(1200));

                Assert.That(
                    afterUpdate.CategoryStatistics,
                    Is.Empty);

                Transaction storedTransaction =
                    await context.Repository.GetByIdAsync(transaction.Id);

                Assert.That(
                    storedTransaction.AmountInMinorUnits,
                    Is.EqualTo(1200));

                Assert.That(
                    storedTransaction.Type,
                    Is.EqualTo(TransactionType.Income));

                Assert.That(
                    storedTransaction.PaymentMethod,
                    Is.EqualTo(PaymentMethod.MobilePayment));
            });
        }

        // Test 3: Deleted transactions are excluded.
        [Test]
        public async Task ExecuteMonthlyAsync_AfterDeletingTransaction_ExcludesDeletedData()
        {
            await RunWithDatabaseAsync(async context =>
            {
                Transaction foodTransaction = CreateTransaction(
                    600,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    AtJapanTime(2026, 11, 2, 12, 0));

                Transaction transportTransaction = CreateTransaction(
                    400,
                    TransactionType.Expense,
                    TransactionCategory.Transport,
                    AtJapanTime(2026, 11, 3, 15, 0));

                await context.Repository.AddAsync(foodTransaction);
                await context.Repository.AddAsync(transportTransaction);

                StatisticsReport beforeDelete =
                    await GetMonthlyReportAsync(context, 2026, 11);

                Assert.That(
                    beforeDelete.TotalExpenseInMinorUnits,
                    Is.EqualTo(1000));

                await context.Repository.DeleteAsync(foodTransaction.Id);

                StatisticsReport afterDelete =
                    await GetMonthlyReportAsync(context, 2026, 11);

                Assert.That(
                    afterDelete.TotalExpenseInMinorUnits,
                    Is.EqualTo(400));

                Assert.That(
                    afterDelete.CategoryStatistics.Count,
                    Is.EqualTo(1));

                Assert.That(
                    afterDelete.CategoryStatistics[0].Category,
                    Is.EqualTo(TransactionCategory.Transport));

                Assert.That(
                    afterDelete.CategoryStatistics[0].ExpenseRatio,
                    Is.EqualTo(1m));
            });
        }

        // Test 4: Added, updated and deleted data persists after reopening.
        [Test]
        public async Task ExecuteMonthlyAsync_AfterReopeningDatabase_PreservesChanges()
        {
            await RunWithDatabaseAsync(async context =>
            {
                Transaction incomeTransaction = CreateTransaction(
                    5000,
                    TransactionType.Income,
                    TransactionCategory.Other,
                    AtJapanTime(2026, 11, 1, 9, 0));

                Transaction updatedTransaction = CreateTransaction(
                    1000,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    AtJapanTime(2026, 11, 5, 12, 0));

                Transaction deletedTransaction = CreateTransaction(
                    300,
                    TransactionType.Expense,
                    TransactionCategory.Transport,
                    AtJapanTime(2026, 11, 6, 14, 0));

                await context.Repository.AddAsync(incomeTransaction);
                await context.Repository.AddAsync(updatedTransaction);
                await context.Repository.AddAsync(deletedTransaction);

                updatedTransaction.UpdateDetails(
                    amountInMinorUnits: 700,
                    type: TransactionType.Expense,
                    category: TransactionCategory.Transport,
                    paymentMethod: PaymentMethod.CreditCard,
                    occurredAt: AtJapanTime(2026, 11, 7, 10, 0),
                    note: "Updated transport expense",
                    updatedAt: AtJapanTime(2026, 11, 8, 10, 0));

                await context.Repository.UpdateAsync(updatedTransaction);
                await context.Repository.DeleteAsync(deletedTransaction.Id);

                StatisticsReport beforeReopen =
                    await GetMonthlyReportAsync(context, 2026, 11);

                context.Reopen();

                StatisticsReport afterReopen =
                    await GetMonthlyReportAsync(context, 2026, 11);

                Assert.That(
                    afterReopen.TotalIncomeInMinorUnits,
                    Is.EqualTo(beforeReopen.TotalIncomeInMinorUnits));

                Assert.That(
                    afterReopen.TotalExpenseInMinorUnits,
                    Is.EqualTo(beforeReopen.TotalExpenseInMinorUnits));

                Assert.That(
                    afterReopen.TotalIncomeInMinorUnits,
                    Is.EqualTo(5000));

                Assert.That(
                    afterReopen.TotalExpenseInMinorUnits,
                    Is.EqualTo(700));

                Assert.That(
                    afterReopen.BalanceInMinorUnits,
                    Is.EqualTo(4300));

                Assert.That(
                    afterReopen.CategoryStatistics.Count,
                    Is.EqualTo(1));

                Assert.That(
                    afterReopen.CategoryStatistics[0].Category,
                    Is.EqualTo(TransactionCategory.Transport));

                Transaction storedTransaction =
                    await context.Repository.GetByIdAsync(
                        updatedTransaction.Id);

                Assert.That(
                    storedTransaction.AmountInMinorUnits,
                    Is.EqualTo(700));

                Assert.That(
                    storedTransaction.PaymentMethod,
                    Is.EqualTo(PaymentMethod.CreditCard));

                Assert.ThrowsAsync<KeyNotFoundException>(
                    async () => await context.Repository.GetByIdAsync(
                        deletedTransaction.Id));
            });
        }

        // Test 5: SQLite filters by instant, not displayed date.
        [Test]
        public async Task ExecuteMonthlyAsync_AtMonthBoundaries_FiltersCorrectly()
        {
            await RunWithDatabaseAsync(async context =>
            {
                // Oct 31 23:59:59.999 in Japan.
                await context.Repository.AddAsync(CreateTransaction(
                    100,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    AtUtc(2026, 10, 31, 14, 59, 59)
                        .AddMilliseconds(999)));

                // Nov 1 00:00:00 in Japan.
                await context.Repository.AddAsync(CreateTransaction(
                    200,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    AtUtc(2026, 10, 31, 15, 0, 0)));

                // Nov 30 23:59:59.999 in Japan.
                await context.Repository.AddAsync(CreateTransaction(
                    300,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    AtUtc(2026, 11, 30, 14, 59, 59)
                        .AddMilliseconds(999)));

                // Dec 1 00:00:00 in Japan.
                await context.Repository.AddAsync(CreateTransaction(
                    400,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    AtUtc(2026, 11, 30, 15, 0, 0)));

                StatisticsReport octoberReport =
                    await GetMonthlyReportAsync(context, 2026, 10);

                StatisticsReport novemberReport =
                    await GetMonthlyReportAsync(context, 2026, 11);

                StatisticsReport decemberReport =
                    await GetMonthlyReportAsync(context, 2026, 12);

                Assert.That(
                    octoberReport.TotalExpenseInMinorUnits,
                    Is.EqualTo(100));

                Assert.That(
                    novemberReport.TotalExpenseInMinorUnits,
                    Is.EqualTo(500));

                Assert.That(
                    decemberReport.TotalExpenseInMinorUnits,
                    Is.EqualTo(400));

                Assert.That(
                    novemberReport.DailyStatistics.Count,
                    Is.EqualTo(2));

                Assert.That(
                    novemberReport.DailyStatistics[0].Date,
                    Is.EqualTo(new DateTime(2026, 11, 1)));

                Assert.That(
                    novemberReport.DailyStatistics[1].Date,
                    Is.EqualTo(new DateTime(2026, 11, 30)));
            });
        }

        // Test 6: Changing the transaction date moves it between reports.
        [Test]
        public async Task ExecuteMonthlyAsync_WhenTransactionMovesMonth_UpdatesBothReports()
        {
            await RunWithDatabaseAsync(async context =>
            {
                Transaction transaction = CreateTransaction(
                    700,
                    TransactionType.Expense,
                    TransactionCategory.Food,
                    AtJapanTime(2026, 11, 30, 20, 0));

                await context.Repository.AddAsync(transaction);

                StatisticsReport novemberBefore =
                    await GetMonthlyReportAsync(context, 2026, 11);

                Assert.That(
                    novemberBefore.TotalExpenseInMinorUnits,
                    Is.EqualTo(700));

                transaction.UpdateDetails(
                    amountInMinorUnits: 700,
                    type: TransactionType.Expense,
                    category: TransactionCategory.Food,
                    paymentMethod: PaymentMethod.Cash,
                    occurredAt: AtJapanTime(2026, 12, 1, 9, 0),
                    note: "Moved to December",
                    updatedAt: AtJapanTime(2026, 12, 2, 12, 0));

                await context.Repository.UpdateAsync(transaction);

                StatisticsReport novemberAfter =
                    await GetMonthlyReportAsync(context, 2026, 11);

                StatisticsReport decemberAfter =
                    await GetMonthlyReportAsync(context, 2026, 12);

                Assert.That(
                    novemberAfter.TotalExpenseInMinorUnits,
                    Is.Zero);

                Assert.That(
                    novemberAfter.CategoryStatistics,
                    Is.Empty);

                Assert.That(
                    decemberAfter.TotalExpenseInMinorUnits,
                    Is.EqualTo(700));

                Assert.That(
                    decemberAfter.DailyStatistics[0].Date,
                    Is.EqualTo(new DateTime(2026, 12, 1)));
            });
        }

        private static Task<StatisticsReport> GetMonthlyReportAsync(
            SQLiteStatisticsTestContext context,
            int year,
            int month)
        {
            return context.UseCase.ExecuteMonthlyAsync(
                year,
                month,
                ReportTimeZone);
        }

        private static DateTimeOffset AtJapanTime(
            int year,
            int month,
            int day,
            int hour,
            int minute)
        {
            return new DateTimeOffset(
                year, month, day, hour, minute, 0,
                TimeSpan.FromHours(9));
        }

        private static DateTimeOffset AtUtc(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second)
        {
            return new DateTimeOffset(
                year, month, day, hour, minute, second,
                TimeSpan.Zero);
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
                note: "Statistics integration test",
                createdAt: occurredAt,
                updatedAt: occurredAt);
        }


        private static async Task RunWithDatabaseAsync(
            Func<SQLiteStatisticsTestContext, Task> testAction)
        {
            var context = new SQLiteStatisticsTestContext();

            try
            {
                context.Initialize();

                await testAction(context);
            }
            finally
            {
                context.Dispose();
            }
        }



        private sealed class SQLiteStatisticsTestContext : IDisposable
        {
            private readonly string _databasePath;
            private CatLedgerDatabase _database;
            private bool _disposed;

            public SQLiteTransactionRepository Repository
            {
                get;
                private set;
            }

            public GetStatisticsReportUseCase UseCase
            {
                get;
                private set;
            }

            public SQLiteStatisticsTestContext()
            {
                _databasePath = Path.Combine(
                    Path.GetTempPath(),
                    $"catledger-statistics-{Guid.NewGuid():N}.db3");

                OpenDatabase();
            }

            public void Initialize()
            {
                ThrowIfDisposed();

                _database.Initialize();
            }

            public void Reopen()
            {
                ThrowIfDisposed();

                // Release the existing database connection.
                _database.Dispose();
                _database = null;

                // Recreate the database, repository and use case.
                OpenDatabase();

                _database.Initialize();
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                try
                {
                    _database?.Dispose();
                }
                finally
                {
                    _database = null;
                    Repository = null;
                    UseCase = null;

                    DeleteIfExists(_databasePath);
                    DeleteIfExists(_databasePath + "-wal");
                    DeleteIfExists(_databasePath + "-shm");
                }
            }

            private void OpenDatabase()
            {
                _database = new CatLedgerDatabase(_databasePath);

                Repository = new SQLiteTransactionRepository(
                    _database);

                UseCase = new GetStatisticsReportUseCase(
                    Repository,
                    new StatisticsCalculator());
            }

            private void ThrowIfDisposed()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(
                        nameof(SQLiteStatisticsTestContext));
                }
            }

            private static void DeleteIfExists(string filePath)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

    }
}
