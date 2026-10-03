using System;
using System.Globalization;
using CatLedger.Application.Finance;
using CatLedger.Domain.Finance;

namespace CatLedger.Presentation.Finance.TransactionForm
{
    public static class TransactionFormMapper
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string TimeFormat = "HH:mm";

        public static TransactionDetails ToTransactionDetails(
            TransactionFormData formData)
        {
            if (formData == null)
            {
                throw new ArgumentNullException(
                    nameof(formData));
            }

            long amountInMinorUnits =
                ParseAmountInMinorUnits(
                    formData.AmountText);

            TransactionType transactionType =
                ParseEnumValue<TransactionType>(
                    formData.TransactionTypeValue);

            TransactionCategory category =
                ParseEnumValue<TransactionCategory>(
                    formData.CategoryValue);

            PaymentMethod paymentMethod =
                ParseEnumValue<PaymentMethod>(
                    formData.PaymentMethodValue);

            DateTimeOffset occurredAt =
                ParseOccurredAt(
                    formData.DateText,
                    formData.TimeText);

            return new TransactionDetails(
                amountInMinorUnits,
                transactionType,
                category,
                paymentMethod,
                occurredAt,
                formData.NoteText);
        }

        public static TransactionFormData FromTransaction(
            Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(
                    nameof(transaction));
            }

            return new TransactionFormData(
                transaction.AmountInMinorUnits.ToString(
                    CultureInfo.InvariantCulture),
                (int)transaction.Type,
                (int)transaction.Category,
                (int)transaction.PaymentMethod,
                transaction.OccurredAt.ToString(
                    DateFormat,
                    CultureInfo.InvariantCulture),
                transaction.OccurredAt.ToString(
                    TimeFormat,
                    CultureInfo.InvariantCulture),
                transaction.Note);
        }

        public static TransactionFormData CreateNewTransactionDefaults(
            DateTimeOffset currentTime)
        {
            return new TransactionFormData(
                amountText: string.Empty,
                transactionTypeValue:
                    (int)TransactionType.Expense,
                categoryValue:
                    (int)TransactionCategory.Food,
                paymentMethodValue:
                    (int)PaymentMethod.Cash,
                dateText:
                    currentTime.ToString(
                        DateFormat,
                        CultureInfo.InvariantCulture),
                timeText:
                    currentTime.ToString(
                        TimeFormat,
                        CultureInfo.InvariantCulture),
                noteText: string.Empty);
        }

        private static long ParseAmountInMinorUnits(
            string amountText)
        {
            if (!long.TryParse(
                    amountText,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
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

        private static DateTimeOffset ParseOccurredAt(
            string dateText,
            string timeText)
        {
            string dateTimeText =
                $"{dateText} {timeText}";

            string expectedFormat =
                $"{DateFormat} {TimeFormat}";

            if (!DateTime.TryParseExact(
                    dateTimeText,
                    expectedFormat,
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

        private static TEnum ParseEnumValue<TEnum>(
            int value)
            where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(
                    typeof(TEnum),
                    value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"Invalid {typeof(TEnum).Name} value.");
            }

            return (TEnum)Enum.ToObject(
                typeof(TEnum),
                value);
        }
    }
}