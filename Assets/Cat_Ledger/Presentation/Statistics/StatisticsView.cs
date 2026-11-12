
using System;
using System.Globalization;
using CatLedger.Application.Statistics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatLedger.Presentation.Statistics
{
    public sealed class StatisticsView : MonoBehaviour
    {
        [Header("Mode Selection")]
        [SerializeField] private Button _monthlyModeButton;
        [SerializeField] private Button _dailyModeButton;
        [SerializeField] private GameObject _monthlySelectedIndicator;
        [SerializeField] private GameObject _dailySelectedIndicator;

        [Header("Period Navigation")]
        [SerializeField] private Button _previousPeriodButton;
        [SerializeField] private Button _nextPeriodButton;
        [SerializeField] private TMP_Text _periodText;

        [Header("State Roots")]
        [SerializeField] private GameObject _loadingRoot;
        [SerializeField] private GameObject _emptyRoot;
        [SerializeField] private GameObject _errorRoot;
        [SerializeField] private GameObject _dataRoot;

        [Header("State Messages")]
        [SerializeField] private TMP_Text _emptyMessageText;
        [SerializeField] private TMP_Text _errorMessageText;
        [SerializeField] private Button _retryButton;

        [Header("Statistics Summary")]
        [SerializeField] private TMP_Text _incomeAmountText;
        [SerializeField] private TMP_Text _expenseAmountText;
        [SerializeField] private TMP_Text _balanceAmountText;

        [Header("Currency Display")]
        [SerializeField] private string _currencySymbol = "¥";

        [Range(0, 3)]
        [SerializeField] private int _currencyFractionDigits = 0;

        public event Action Shown;
        public event Action Hidden;

        public event Action MonthlyModeRequested;
        public event Action DailyModeRequested;

        public event Action PreviousPeriodRequested;
        public event Action NextPeriodRequested;
        public event Action RetryRequested;

        private void Awake()
        {
            _monthlyModeButton.onClick.AddListener(
                OnMonthlyModeClicked);

            _dailyModeButton.onClick.AddListener(
                OnDailyModeClicked);

            _previousPeriodButton.onClick.AddListener(
                OnPreviousPeriodClicked);

            _nextPeriodButton.onClick.AddListener(
                OnNextPeriodClicked);

            _retryButton.onClick.AddListener(
                OnRetryClicked);
        }

        private void OnEnable()
        {
            Shown?.Invoke();
        }

        private void OnDisable()
        {
            Hidden?.Invoke();
        }

        private void OnDestroy()
        {
            _monthlyModeButton.onClick.RemoveListener(
                OnMonthlyModeClicked);

            _dailyModeButton.onClick.RemoveListener(
                OnDailyModeClicked);

            _previousPeriodButton.onClick.RemoveListener(
                OnPreviousPeriodClicked);

            _nextPeriodButton.onClick.RemoveListener(
                OnNextPeriodClicked);

            _retryButton.onClick.RemoveListener(
                OnRetryClicked);
        }

        public void SetPeriod(
            DateTime selectedDate,
            bool isMonthly)
        {
            _periodText.text = selectedDate.ToString(
                isMonthly
                    ? "yyyy 年 M 月"
                    : "yyyy 年 M 月 d 日",
                CultureInfo.InvariantCulture);

            _monthlySelectedIndicator.SetActive(isMonthly);
            _dailySelectedIndicator.SetActive(!isMonthly);
        }

        public void ShowLoading()
        {
            SetState(_loadingRoot);
        }

        public void ShowEmpty(bool isMonthly)
        {
            _emptyMessageText.text = isMonthly
                ? "這個月份還沒有記帳紀錄"
                : "這一天還沒有記帳紀錄";

            SetState(_emptyRoot);
        }

        public void ShowError(string message)
        {
            _errorMessageText.text = message;
            SetState(_errorRoot);
        }

        public void ShowReport(StatisticsReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            _incomeAmountText.text = FormatAmount(
                report.TotalIncomeInMinorUnits);

            _expenseAmountText.text = FormatAmount(
                report.TotalExpenseInMinorUnits);

            _balanceAmountText.text = FormatAmount(
                report.BalanceInMinorUnits);

            SetState(_dataRoot);
        }

        private void SetState(GameObject activeRoot)
        {
            _loadingRoot.SetActive(activeRoot == _loadingRoot);
            _emptyRoot.SetActive(activeRoot == _emptyRoot);
            _errorRoot.SetActive(activeRoot == _errorRoot);
            _dataRoot.SetActive(activeRoot == _dataRoot);
        }

        private string FormatAmount(long amountInMinorUnits)
        {
            decimal minorUnitsPerMajorUnit = 1m;

            for (int index = 0;
                 index < _currencyFractionDigits;
                 index++)
            {
                minorUnitsPerMajorUnit *= 10m;
            }

            decimal amountInMajorUnits =
                amountInMinorUnits / minorUnitsPerMajorUnit;

            string sign = amountInMinorUnits < 0
                ? "-"
                : string.Empty;

            string formattedAmount = Math.Abs(amountInMajorUnits)
                .ToString(
                    "N" + _currencyFractionDigits,
                    CultureInfo.CurrentCulture);

            return $"{sign}{_currencySymbol}{formattedAmount}";
        }

        private void OnMonthlyModeClicked()
        {
            MonthlyModeRequested?.Invoke();
        }

        private void OnDailyModeClicked()
        {
            DailyModeRequested?.Invoke();
        }

        private void OnPreviousPeriodClicked()
        {
            PreviousPeriodRequested?.Invoke();
        }

        private void OnNextPeriodClicked()
        {
            NextPeriodRequested?.Invoke();
        }

        private void OnRetryClicked()
        {
            RetryRequested?.Invoke();
        }
    }
}
