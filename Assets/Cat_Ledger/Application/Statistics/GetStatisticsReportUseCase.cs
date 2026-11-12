
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatLedger.Application.Finance;
using CatLedger.Domain.Finance;

namespace CatLedger.Application.Statistics
{
    public sealed class GetStatisticsReportUseCase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly StatisticsCalculator _statisticsCalculator;

        public GetStatisticsReportUseCase(
            ITransactionRepository transactionRepository,
            StatisticsCalculator statisticsCalculator)
        {
            _transactionRepository = transactionRepository
                ?? throw new ArgumentNullException(
                    nameof(transactionRepository));

            _statisticsCalculator = statisticsCalculator
                ?? throw new ArgumentNullException(
                    nameof(statisticsCalculator));
        }

        public Task<StatisticsReport> ExecuteMonthlyAsync(
            int year,
            int month,
            TimeZoneInfo reportTimeZone)
        {
            ValidateTimeZone(reportTimeZone);

            DateTime localStart = new DateTime(
                year,
                month,
                1);

            DateTime localEnd = localStart.AddMonths(1);

            return ExecuteAsync(
                localStart,
                localEnd,
                reportTimeZone);
        }

        public Task<StatisticsReport> ExecuteDailyAsync(
            int year,
            int month,
            int day,
            TimeZoneInfo reportTimeZone)
        {
            ValidateTimeZone(reportTimeZone);

            DateTime localStart = new DateTime(
                year,
                month,
                day);

            DateTime localEnd = localStart.AddDays(1);

            return ExecuteAsync(
                localStart,
                localEnd,
                reportTimeZone);
        }

        private async Task<StatisticsReport> ExecuteAsync(
            DateTime localStart,
            DateTime localEnd,
            TimeZoneInfo reportTimeZone)
        {
            DateTimeOffset startOccurredAtInclusive =
                CreateDateBoundary(localStart, reportTimeZone);

            DateTimeOffset endOccurredAtExclusive =
                CreateDateBoundary(localEnd, reportTimeZone);

            var filter = new TransactionFilter(
                startOccurredAtInclusive: startOccurredAtInclusive,
                endOccurredAtExclusive: endOccurredAtExclusive);

            IReadOnlyList<Transaction> transactions =
                await _transactionRepository.GetMatchingAsync(filter);

            return _statisticsCalculator.Calculate(
                transactions,
                startOccurredAtInclusive,
                endOccurredAtExclusive,
                reportTimeZone);
        }

        private static void ValidateTimeZone(
            TimeZoneInfo reportTimeZone)
        {
            if (reportTimeZone == null)
            {
                throw new ArgumentNullException(
                    nameof(reportTimeZone));
            }
        }

        private static DateTimeOffset CreateDateBoundary(
            DateTime localDate,
            TimeZoneInfo reportTimeZone)
        {
            DateTime localBoundary = DateTime.SpecifyKind(
                localDate.Date,
                DateTimeKind.Unspecified);

            // Some time zones skip local midnight during DST changes.
            // Move to the first valid local minute of that date.
            while (reportTimeZone.IsInvalidTime(localBoundary))
            {
                localBoundary = localBoundary.AddMinutes(1);

                if (localBoundary.Date != localDate.Date)
                {
                    throw new InvalidOperationException(
                        "The requested calendar date does not exist " +
                        "in the report time zone.");
                }
            }

            TimeSpan utcOffset;

            if (reportTimeZone.IsAmbiguousTime(localBoundary))
            {
                TimeSpan[] possibleOffsets =
                    reportTimeZone.GetAmbiguousTimeOffsets(localBoundary);

                // A larger UTC offset represents the earlier occurrence
                // of an ambiguous local time.
                utcOffset = possibleOffsets[0] > possibleOffsets[1]
                    ? possibleOffsets[0]
                    : possibleOffsets[1];
            }
            else
            {
                utcOffset = reportTimeZone.GetUtcOffset(localBoundary);
            }

            return new DateTimeOffset(localBoundary, utcOffset);
        }
    }
}
