using System;
using System.Globalization;
using CatLedger.Application.Common;
using CatLedger.Application.Finance;
using CatLedger.Domain.Finance;
using VContainer.Unity;

namespace CatLedger.Presentation.Finance.AddTransaction
{
    public sealed class AddTransactionPresenter :
        IStartable,
        IDisposable
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string TimeFormat = "HH:mm";

        private readonly AddTransactionView _view;
        private readonly AddTransactionUseCase _addTransactionUseCase;
        private readonly IClock _clock;

        private bool _isSaving;

        public AddTransactionPresenter(
    AddTransactionView view,
    AddTransactionUseCase addTransactionUseCase,
    IClock clock)
        {
            _view = view
                ?? throw new ArgumentNullException(
                    nameof(view));

            _addTransactionUseCase = addTransactionUseCase
                ?? throw new ArgumentNullException(
                    nameof(addTransactionUseCase));

            _clock = clock
                ?? throw new ArgumentNullException(
                    nameof(clock));

        }

        public void Start()
        {
            _view.SaveRequested += HandleSaveRequested;

            InitializeDefaultDateTime();
        }

        public void Dispose()
        {
            _view.SaveRequested -= HandleSaveRequested;
        }

        private async void HandleSaveRequested()
        {
            if (_isSaving)
                return;

            try
            {
                _isSaving = true;
                _view.SetSaveButtonInteractable(false);

                TransactionDetails transactionDetails =
                    CreateTransactionDetails();

                var request =
                    new AddTransactionRequest(
                        transactionDetails);

                await _addTransactionUseCase.ExecuteAsync(request);

                _view.ShowSuccess("Transaction saved.");

                _view.ClearEditableFields();
            }
            catch (Exception exception)
            {
                _view.ShowError(exception.Message);
            }
            finally
            {
                _isSaving = false;
                _view.SetSaveButtonInteractable(true);
            }
        }

        private TransactionDetails CreateTransactionDetails()
        {
            long amountInMinorUnits =
                ParseAmountInMinorUnits();

            TransactionType transactionType =
                ParseEnumSelection<TransactionType>(
                    _view.SelectedTransactionTypeIndex);

            TransactionCategory category =
                ParseEnumSelection<TransactionCategory>(
                    _view.SelectedCategoryIndex);

            PaymentMethod paymentMethod =
                ParseEnumSelection<PaymentMethod>(
                    _view.SelectedPaymentMethodIndex);

            DateTimeOffset occurredAt =
                ParseOccurredAt();

            return new TransactionDetails(
                amountInMinorUnits,
                transactionType,
                category,
                paymentMethod,
                occurredAt,
                _view.NoteText);
        }

        private long ParseAmountInMinorUnits()
        {
            if (!long.TryParse(
                    _view.AmountText,
                    out long amountInMinorUnits))
            {
                throw new ArgumentException(
                    "Please enter a valid amount.");
            }

            if (amountInMinorUnits <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amountInMinorUnits),
                    "Amount must be greater than zero.");
            }

            return amountInMinorUnits;
        }

        private DateTimeOffset ParseOccurredAt()
        {
            string dateTimeText =
                $"{_view.DateText} {_view.TimeText}";

            string dateTimeFormat =
                $"{DateFormat} {TimeFormat}";

            if (!DateTime.TryParseExact(
                    dateTimeText,
                    dateTimeFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime localDateTime))
            {
                throw new ArgumentException(
                    $"Date and time must use " +
                    $"{DateFormat} and {TimeFormat}.");
            }

            TimeSpan localUtcOffset =
                TimeZoneInfo.Local.GetUtcOffset(
                    localDateTime);

            return new DateTimeOffset(
                localDateTime,
                localUtcOffset);
        }

        private static TEnum ParseEnumSelection<TEnum>(
            int selectedValue)
            where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(
                    typeof(TEnum),
                    selectedValue))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(selectedValue),
                    $"Invalid {typeof(TEnum).Name} selection.");
            }

            return (TEnum)Enum.ToObject(
                typeof(TEnum),
                selectedValue);
        }

        private void InitializeDefaultDateTime()
        {
            DateTimeOffset currentTime =
                _clock.Now;

            _view.SetDate(
                currentTime.ToString(
                    DateFormat,
                    CultureInfo.InvariantCulture));

            _view.SetTime(
                currentTime.ToString(
                    TimeFormat,
                    CultureInfo.InvariantCulture));
        }
    }
}