using System;
using CatLedger.Domain.Finance;
using CatLedger.Infrastructure.Persistence.SQLite.Records;

namespace CatLedger.Infrastructure.Persistence.SQLite.Mapping
{
    internal static class TransactionRecordMapper
    {
        public static TransactionRecord ToRecord(Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            return new TransactionRecord
            {
                Id = transaction.Id.ToString(),
                AmountInMinorUnits = transaction.AmountInMinorUnits,
                Type = (int)transaction.Type,
                Category = (int)transaction.Category,
                PaymentMethod = (int)transaction.PaymentMethod,

                OccurredAtUnixMilliseconds =
                    transaction.OccurredAt.ToUnixTimeMilliseconds(),

                OccurredAtOffsetMinutes =
                    ConvertOffsetToMinutes(transaction.OccurredAt),

                CreatedAtUnixMilliseconds =
                    transaction.CreatedAt.ToUnixTimeMilliseconds(),

                CreatedAtOffsetMinutes =
                    ConvertOffsetToMinutes(transaction.CreatedAt),

                UpdatedAtUnixMilliseconds =
                    transaction.UpdatedAt.ToUnixTimeMilliseconds(),

                UpdatedAtOffsetMinutes =
                    ConvertOffsetToMinutes(transaction.UpdatedAt),

                Note = transaction.Note
            };
        }

        /// <summary>
        /// 資料庫資料轉換為APP使用資料
        /// </summary>
        public static Transaction ToDomain(TransactionRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            if (!Guid.TryParse(record.Id, out Guid transactionId))
            {
                throw new InvalidOperationException(
                    $"Invalid transaction ID stored in database: {record.Id}");
            }

            TransactionType transactionType =
                ParseEnum<TransactionType>(record.Type);

            TransactionCategory category =
                ParseEnum<TransactionCategory>(record.Category);

            PaymentMethod paymentMethod =
                ParseEnum<PaymentMethod>(record.PaymentMethod);

            return new Transaction(
                transactionId,
                record.AmountInMinorUnits,
                transactionType,
                category,
                paymentMethod,
                RestoreDateTimeOffset(
                    record.OccurredAtUnixMilliseconds,
                    record.OccurredAtOffsetMinutes),
                record.Note,
                RestoreDateTimeOffset(
                    record.CreatedAtUnixMilliseconds,
                    record.CreatedAtOffsetMinutes),
                RestoreDateTimeOffset(
                    record.UpdatedAtUnixMilliseconds,
                    record.UpdatedAtOffsetMinutes));
        }

        private static int ConvertOffsetToMinutes(DateTimeOffset dateTime)
        {
            return (int)dateTime.Offset.TotalMinutes;
        }

        private static DateTimeOffset RestoreDateTimeOffset(
            long unixMilliseconds,
            int offsetMinutes)
        {
            TimeSpan offset = TimeSpan.FromMinutes(offsetMinutes);

            return DateTimeOffset
                .FromUnixTimeMilliseconds(unixMilliseconds)
                .ToOffset(offset);
        }

        private static TEnum ParseEnum<TEnum>(int storedValue)
            where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(typeof(TEnum), storedValue))
            {
                throw new InvalidOperationException(
                    $"Invalid {typeof(TEnum).Name} value stored in database: {storedValue}");
            }

            return (TEnum)Enum.ToObject(typeof(TEnum), storedValue);
        }
    }
}