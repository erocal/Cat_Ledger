using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatLedger.Application.Finance;
using CatLedger.Application.Statistics;
using CatLedger.Domain.Finance;
using NUnit.Framework;

namespace CatLedger.Tests.EditMode.Statistics
{
    public sealed class GetStatisticsReportUseCaseTests
    {
        private static readonly TimeZoneInfo JapanTimeZone =
            TimeZoneInfo.CreateCustomTimeZone(
                "CatLedger.Test.UTC+09",
                TimeSpan.FromHours(9),
                "Japan Test Time Zone",
                "UTC+09");

        [Test]
        public async Task ExecuteMonthlyAsync_FiltersTransactionsByMonth()
        {
            var repository = new FakeTransactionRepository(
                CreateExpense(
                    100,
                    new DateTimeOffset(
                        2026, 10, 31, 23, 59, 0,
                        TimeSpan.FromHours(9))),
                CreateExpense(
                    200,
                    new DateTimeOffset(
                        2026, 11, 1, 0, 0, 0,
                        TimeSpan.FromHours(9))),
                CreateExpense(
                    300,
                    new DateTimeOffset(
                        2026, 11, 30, 23, 59, 0,
                        TimeSpan.FromHours(9))),
                CreateExpense(
                    400,
                    new DateTimeOffset(
                        2026, 12, 1, 0, 0, 0,
                        TimeSpan.FromHours(9))));

            var useCase = CreateUseCase(repository);

            StatisticsReport report =
                await useCase.ExecuteMonthlyAsync(
                    2026, 11, JapanTimeZone);

            Assert.That(report.TotalExpenseInMinorUnits,
                Is.EqualTo(500));

            Assert.That(repository.QueryCount, Is.EqualTo(1));

            Assert.That(
                repository.LastFilter.StartOccurredAtInclusive.Value,
                Is.EqualTo(new DateTimeOffset(
                    2026, 11, 1, 0, 0, 0,
                    TimeSpan.FromHours(9))));

            Assert.That(
                repository.LastFilter.EndOccurredAtExclusive.Value,
                Is.EqualTo(new DateTimeOffset(
                    2026, 12, 1, 0, 0, 0,
                    TimeSpan.FromHours(9))));
        }

        [Test]
        public async Task ExecuteMonthlyAsync_AcrossYear_UsesNextJanuary()
        {
            var repository = new FakeTransactionRepository(
                CreateExpense(
                    100,
                    new DateTimeOffset(
                        2026, 12, 31, 20, 0, 0,
                        TimeSpan.FromHours(9))),
                CreateExpense(
                    200,
                    new DateTimeOffset(
                        2027, 1, 1, 0, 0, 0,
                        TimeSpan.FromHours(9))));

            var useCase = CreateUseCase(repository);

            StatisticsReport report =
                await useCase.ExecuteMonthlyAsync(
                    2026, 12, JapanTimeZone);

            Assert.That(report.TotalExpenseInMinorUnits,
                Is.EqualTo(100));

            Assert.That(
                report.EndOccurredAtExclusive,
                Is.EqualTo(new DateTimeOffset(
                    2027, 1, 1, 0, 0, 0,
                    TimeSpan.FromHours(9))));
        }

        [Test]
        public async Task ExecuteDailyAsync_UsesLocalCalendarDate()
        {
            var repository = new FakeTransactionRepository(
                CreateExpense(
                    100,
                    new DateTimeOffset(
                        2026, 11, 30, 14, 30, 0,
                        TimeSpan.Zero)),
                CreateExpense(
                    300,
                    new DateTimeOffset(
                        2026, 11, 30, 15, 30, 0,
                        TimeSpan.Zero)));

            var useCase = CreateUseCase(repository);

            StatisticsReport report =
                await useCase.ExecuteDailyAsync(
                    2026, 12, 1, JapanTimeZone);

            Assert.That(report.TotalExpenseInMinorUnits,
                Is.EqualTo(300));

            Assert.That(report.DailyStatistics.Count,
                Is.EqualTo(1));

            Assert.That(report.DailyStatistics[0].Date,
                Is.EqualTo(new DateTime(2026, 12, 1)));
        }

        [Test]
        public async Task ExecuteMonthlyAsync_AcrossDstStart_UsesDifferentOffsets()
        {
            TimeZoneInfo timeZone = CreateEasternTimeZone();

            var repository = new FakeTransactionRepository(
                CreateExpense(
                    100,
                    new DateTimeOffset(
                        2026, 3, 8, 6, 59, 0,
                        TimeSpan.Zero)),
                CreateExpense(
                    200,
                    new DateTimeOffset(
                        2026, 3, 8, 7, 1, 0,
                        TimeSpan.Zero)));

            var useCase = CreateUseCase(repository);

            StatisticsReport report =
                await useCase.ExecuteMonthlyAsync(
                    2026, 3, timeZone);

            Assert.That(report.TotalExpenseInMinorUnits,
                Is.EqualTo(300));

            Assert.That(report.StartOccurredAtInclusive,
                Is.EqualTo(new DateTimeOffset(
                    2026, 3, 1, 0, 0, 0,
                    TimeSpan.FromHours(-5))));

            Assert.That(report.EndOccurredAtExclusive,
                Is.EqualTo(new DateTimeOffset(
                    2026, 4, 1, 0, 0, 0,
                    TimeSpan.FromHours(-4))));

            Assert.That(
                report.EndOccurredAtExclusive -
                report.StartOccurredAtInclusive,
                Is.EqualTo(TimeSpan.FromHours(743)));
        }

        [Test]
        public async Task ExecuteDailyAsync_AcrossDstEnd_IncludesRepeatedHour()
        {
            TimeZoneInfo timeZone = CreateEasternTimeZone();

            var repository = new FakeTransactionRepository(
                // 01:30 before clocks are turned back.
                CreateExpense(
                    100,
                    new DateTimeOffset(
                        2026, 11, 1, 5, 30, 0,
                        TimeSpan.Zero)),

                // 01:30 after clocks are turned back.
                CreateExpense(
                    200,
                    new DateTimeOffset(
                        2026, 11, 1, 6, 30, 0,
                        TimeSpan.Zero)));

            var useCase = CreateUseCase(repository);

            StatisticsReport report =
                await useCase.ExecuteDailyAsync(
                    2026, 11, 1, timeZone);

            Assert.That(report.TotalExpenseInMinorUnits,
                Is.EqualTo(300));

            Assert.That(report.StartOccurredAtInclusive,
                Is.EqualTo(new DateTimeOffset(
                    2026, 11, 1, 0, 0, 0,
                    TimeSpan.FromHours(-4))));

            Assert.That(report.EndOccurredAtExclusive,
                Is.EqualTo(new DateTimeOffset(
                    2026, 11, 2, 0, 0, 0,
                    TimeSpan.FromHours(-5))));

            Assert.That(
                report.EndOccurredAtExclusive -
                report.StartOccurredAtInclusive,
                Is.EqualTo(TimeSpan.FromHours(25)));
        }

        [Test]
        public async Task ExecuteMonthlyAsync_WithInvalidMidnight_UsesFirstValidTime()
        {
            TimeZoneInfo timeZone =
                CreateTimeZoneWithMidnightDstStart();

            Assert.That(
                timeZone.IsInvalidTime(new DateTime(2026, 3, 1)),
                Is.True);

            var repository = new FakeTransactionRepository(
                CreateExpense(
                    100,
                    new DateTimeOffset(
                        2026, 3, 1, 5, 15, 0,
                        TimeSpan.Zero)));

            var useCase = CreateUseCase(repository);

            StatisticsReport report =
                await useCase.ExecuteMonthlyAsync(
                    2026, 3, timeZone);

            Assert.That(report.StartOccurredAtInclusive,
                Is.EqualTo(new DateTimeOffset(
                    2026, 3, 1, 1, 0, 0,
                    TimeSpan.FromHours(-4))));

            Assert.That(report.TotalExpenseInMinorUnits,
                Is.EqualTo(100));
        }

        [Test]
        public void ExecuteMonthlyAsync_WithInvalidMonth_ThrowsException()
        {
            var repository = new FakeTransactionRepository();
            var useCase = CreateUseCase(repository);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                useCase.ExecuteMonthlyAsync(
                    2026, 13, JapanTimeZone));

            Assert.That(repository.QueryCount, Is.Zero);
        }

        [Test]
        public void ExecuteDailyAsync_WithInvalidDay_ThrowsException()
        {
            var repository = new FakeTransactionRepository();
            var useCase = CreateUseCase(repository);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                useCase.ExecuteDailyAsync(
                    2026, 2, 30, JapanTimeZone));

            Assert.That(repository.QueryCount, Is.Zero);
        }

        [Test]
        public void ExecuteMonthlyAsync_WithNullTimeZone_ThrowsException()
        {
            var repository = new FakeTransactionRepository();
            var useCase = CreateUseCase(repository);

            Assert.Throws<ArgumentNullException>(() =>
                useCase.ExecuteMonthlyAsync(
                    2026, 11, null));

            Assert.That(repository.QueryCount, Is.Zero);
        }

        [Test]
        public void ExecuteMonthlyAsync_WhenRepositoryFails_PropagatesException()
        {
            var repository = new FakeTransactionRepository
            {
                FailOnQuery = true
            };

            var useCase = CreateUseCase(repository);

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await useCase.ExecuteMonthlyAsync(
                    2026, 11, JapanTimeZone));
        }

        private static GetStatisticsReportUseCase CreateUseCase(
            ITransactionRepository repository)
        {
            return new GetStatisticsReportUseCase(
                repository,
                new StatisticsCalculator());
        }

        private static Transaction CreateExpense(
            long amountInMinorUnits,
            DateTimeOffset occurredAt)
        {
            return new Transaction(
                Guid.NewGuid(),
                amountInMinorUnits: amountInMinorUnits,
                type: TransactionType.Expense,
                category: TransactionCategory.Food,
                paymentMethod: PaymentMethod.Cash,
                occurredAt: occurredAt,
                note: "Statistics use case test",
                createdAt: occurredAt,
                updatedAt: occurredAt);
        }

        private static TimeZoneInfo CreateEasternTimeZone()
        {
            TimeZoneInfo.TransitionTime daylightStart =
                TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
                    new DateTime(1, 1, 1, 2, 0, 0),
                    3,
                    2,
                    DayOfWeek.Sunday);

            TimeZoneInfo.TransitionTime daylightEnd =
                TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
                    new DateTime(1, 1, 1, 2, 0, 0),
                    11,
                    1,
                    DayOfWeek.Sunday);

            return CreateTestDaylightTimeZone(
                "CatLedger.Test.Eastern",
                daylightStart,
                daylightEnd);
        }

        private static TimeZoneInfo CreateTimeZoneWithMidnightDstStart()
        {
            TimeZoneInfo.TransitionTime daylightStart =
                TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
                    new DateTime(1, 1, 1, 0, 0, 0),
                    3,
                    1,
                    DayOfWeek.Sunday);

            TimeZoneInfo.TransitionTime daylightEnd =
                TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
                    new DateTime(1, 1, 1, 2, 0, 0),
                    11,
                    1,
                    DayOfWeek.Sunday);

            return CreateTestDaylightTimeZone(
                "CatLedger.Test.MidnightDST",
                daylightStart,
                daylightEnd);
        }

        private static TimeZoneInfo CreateTestDaylightTimeZone(
            string id,
            TimeZoneInfo.TransitionTime daylightStart,
            TimeZoneInfo.TransitionTime daylightEnd)
        {
            TimeZoneInfo.AdjustmentRule rule =
                TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                    new DateTime(2020, 1, 1),
                    new DateTime(2035, 12, 31),
                    TimeSpan.FromHours(1),
                    daylightStart,
                    daylightEnd);

            return TimeZoneInfo.CreateCustomTimeZone(
                id,
                TimeSpan.FromHours(-5),
                id,
                "Standard Time",
                "Daylight Time",
                new[] { rule });
        }

        private sealed class FakeTransactionRepository
            : ITransactionRepository
        {
            private readonly IReadOnlyList<Transaction> _transactions;

            public TransactionFilter LastFilter { get; private set; }

            public int QueryCount { get; private set; }

            public bool FailOnQuery { get; set; }

            public FakeTransactionRepository(
                params Transaction[] transactions)
            {
                _transactions = transactions;
            }

            public Task<IReadOnlyList<Transaction>> GetMatchingAsync(
                TransactionFilter filter)
            {
                QueryCount++;
                LastFilter = filter;

                if (FailOnQuery)
                {
                    return Task.FromException<IReadOnlyList<Transaction>>(
                        new InvalidOperationException(
                            "Simulated repository failure."));
                }

                IReadOnlyList<Transaction> matchingTransactions =
                    _transactions
                        .Where(transaction =>
                            (!filter.StartOccurredAtInclusive.HasValue ||
                             transaction.OccurredAt >=
                             filter.StartOccurredAtInclusive.Value) &&
                            (!filter.EndOccurredAtExclusive.HasValue ||
                             transaction.OccurredAt <
                             filter.EndOccurredAtExclusive.Value))
                        .ToList();

                return Task.FromResult(matchingTransactions);
            }

            public Task AddAsync(Transaction transaction)
            {
                throw new NotSupportedException();
            }

            public Task<Transaction> GetByIdAsync(Guid transactionId)
            {
                throw new NotSupportedException();
            }

            public Task UpdateAsync(Transaction transaction)
            {
                throw new NotSupportedException();
            }

            public Task DeleteAsync(Guid transactionId)
            {
                throw new NotSupportedException();
            }
        }
    }
}
