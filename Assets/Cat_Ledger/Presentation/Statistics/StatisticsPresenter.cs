
using System;
using System.Threading.Tasks;
using CatLedger.Application.Common;
using CatLedger.Application.Statistics;
using UnityEngine;
using VContainer.Unity;

namespace CatLedger.Presentation.Statistics
{
    public sealed class StatisticsPresenter : IStartable, IDisposable
    {
        private readonly StatisticsView _view;
        private readonly GetStatisticsReportUseCase _getStatisticsReportUseCase;
        private readonly IClock _clock;
        private readonly TimeZoneInfo _reportTimeZone;

        private DateTime _selectedDate;
        private bool _isMonthly = true;
        private bool _disposed;
        private int _requestVersion;

        public StatisticsPresenter(
            StatisticsView view,
            GetStatisticsReportUseCase getStatisticsReportUseCase,
            IClock clock)
        {
            _view = view
                ?? throw new ArgumentNullException(nameof(view));

            _getStatisticsReportUseCase = getStatisticsReportUseCase
                ?? throw new ArgumentNullException(
                    nameof(getStatisticsReportUseCase));

            _clock = clock
                ?? throw new ArgumentNullException(nameof(clock));

            _reportTimeZone = TimeZoneInfo.Local;
        }

        public void Start()
        {
            _selectedDate = TimeZoneInfo.ConvertTime(
                _clock.Now,
                _reportTimeZone).Date;

            _view.Shown += OnShown;
            _view.Hidden += OnHidden;

            _view.MonthlyModeRequested += OnMonthlyModeRequested;
            _view.DailyModeRequested += OnDailyModeRequested;

            _view.PreviousPeriodRequested += OnPreviousPeriodRequested;
            _view.NextPeriodRequested += OnNextPeriodRequested;
            _view.RetryRequested += OnRetryRequested;

            // OnEnable might have run before IStartable.Start().
            if (_view.isActiveAndEnabled)
            {
                OnShown();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _requestVersion++;

            if (_view == null)
            {
                return;
            }

            _view.Shown -= OnShown;
            _view.Hidden -= OnHidden;

            _view.MonthlyModeRequested -= OnMonthlyModeRequested;
            _view.DailyModeRequested -= OnDailyModeRequested;

            _view.PreviousPeriodRequested -= OnPreviousPeriodRequested;
            _view.NextPeriodRequested -= OnNextPeriodRequested;
            _view.RetryRequested -= OnRetryRequested;
        }

        private void OnShown()
        {
            RequestRefresh();
        }

        private void OnHidden()
        {
            // Invalidate any result from a previously visible page.
            _requestVersion++;
        }

        private void OnMonthlyModeRequested()
        {
            if (_isMonthly)
            {
                return;
            }

            _isMonthly = true;
            RequestRefresh();
        }

        private void OnDailyModeRequested()
        {
            if (!_isMonthly)
            {
                return;
            }

            _isMonthly = false;
            RequestRefresh();
        }

        private void OnPreviousPeriodRequested()
        {
            ChangePeriod(-1);
        }

        private void OnNextPeriodRequested()
        {
            ChangePeriod(1);
        }

        private void OnRetryRequested()
        {
            RequestRefresh();
        }

        private void ChangePeriod(int direction)
        {
            if (_isMonthly)
            {
                DateTime firstDayOfMonth = new DateTime(
                    _selectedDate.Year,
                    _selectedDate.Month,
                    1);

                _selectedDate = firstDayOfMonth.AddMonths(direction);
            }
            else
            {
                _selectedDate = _selectedDate.AddDays(direction);
            }

            RequestRefresh();
        }

        private void RequestRefresh()
        {
            if (_disposed ||
                _view == null ||
                !_view.isActiveAndEnabled)
            {
                return;
            }

            int currentRequestVersion = ++_requestVersion;

            // Capture values so the async operation always uses
            // the period selected when the request started.
            DateTime requestedDate = _selectedDate;
            bool requestedMonthlyMode = _isMonthly;

            _view.SetPeriod(
                requestedDate,
                requestedMonthlyMode);

            _view.ShowLoading();

            _ = LoadReportAsync(
                currentRequestVersion,
                requestedDate,
                requestedMonthlyMode);
        }

        private async Task LoadReportAsync(
            int requestVersion,
            DateTime requestedDate,
            bool isMonthly)
        {
            try
            {
                StatisticsReport report;

                if (isMonthly)
                {
                    report = await _getStatisticsReportUseCase
                        .ExecuteMonthlyAsync(
                            requestedDate.Year,
                            requestedDate.Month,
                            _reportTimeZone);
                }
                else
                {
                    report = await _getStatisticsReportUseCase
                        .ExecuteDailyAsync(
                            requestedDate.Year,
                            requestedDate.Month,
                            requestedDate.Day,
                            _reportTimeZone);
                }

                if (!IsCurrentRequest(requestVersion))
                {
                    return;
                }

                if (report.DailyStatistics.Count == 0)
                {
                    _view.ShowEmpty(isMonthly);
                    return;
                }

                _view.ShowReport(report);
            }
            catch (Exception exception)
            {
                if (!IsCurrentRequest(requestVersion))
                {
                    return;
                }

                Debug.LogException(exception);

                _view.ShowError(
                    "無法載入統計資料，請確認後重新嘗試。");
            }
        }

        private bool IsCurrentRequest(int requestVersion)
        {
            return !_disposed
                && requestVersion == _requestVersion
                && _view != null
                && _view.isActiveAndEnabled;
        }
    }
}
