using CatLedger.Application.Finance;
using CatLedger.Domain.Finance;
using CatLedger.Infrastructure.Persistence.SQLite.Mapping;
using CatLedger.Infrastructure.Persistence.SQLite.Records;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatLedger.Infrastructure.Persistence.SQLite
{
    public sealed class SQLiteTransactionRepository :
        ITransactionRepository
    {
        private readonly CatLedgerDatabase _database;

        public SQLiteTransactionRepository(
            CatLedgerDatabase database)
        {
            _database = database
                ?? throw new ArgumentNullException(
                    nameof(database));
        }

        public Task AddAsync(
            Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(
                    nameof(transaction));
            }

            _database.Initialize();

            TransactionRecord record =
                TransactionRecordMapper.ToRecord(
                    transaction);

            _database.Connection.Insert(record);

            return Task.CompletedTask;
        }

        public Task<Transaction> GetByIdAsync(
            Guid transactionId)
        {
            if (transactionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Transaction ID cannot be empty.",
                    nameof(transactionId));
            }

            _database.Initialize();

            TransactionRecord record =
                _database.Connection
                    .Find<TransactionRecord>(
                        transactionId.ToString());

            if (record == null)
            {
                throw new KeyNotFoundException(
                    $"Transaction was not found: {transactionId}");
            }

            Transaction transaction =
                TransactionRecordMapper.ToDomain(
                    record);

            return Task.FromResult(transaction);
        }

        public Task<IReadOnlyList<Transaction>>
            GetMatchingAsync(
                TransactionFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(
                    nameof(filter));
            }

            _database.Initialize();

            TableQuery<TransactionRecord> databaseQuery =
                _database.Connection
                    .Table<TransactionRecord>();

            if (filter.StartOccurredAtInclusive.HasValue)
            {
                long startUnixMilliseconds =
                    filter.StartOccurredAtInclusive.Value
                        .ToUnixTimeMilliseconds();

                databaseQuery =
                    databaseQuery.Where(
                        record =>
                            record.OccurredAtUnixMilliseconds >=
                            startUnixMilliseconds);
            }

            if (filter.EndOccurredAtExclusive.HasValue)
            {
                long endUnixMilliseconds =
                    filter.EndOccurredAtExclusive.Value
                        .ToUnixTimeMilliseconds();

                databaseQuery =
                    databaseQuery.Where(
                        record =>
                            record.OccurredAtUnixMilliseconds <
                            endUnixMilliseconds);
            }

            if (filter.Category.HasValue)
            {
                int categoryValue =
                    (int)filter.Category.Value;

                databaseQuery =
                    databaseQuery.Where(
                        record =>
                            record.Category ==
                            categoryValue);
            }

            if (filter.Type.HasValue)
            {
                int transactionTypeValue =
                    (int)filter.Type.Value;

                databaseQuery =
                    databaseQuery.Where(
                        record =>
                            record.Type ==
                            transactionTypeValue);
            }

            List<TransactionRecord> records =
                databaseQuery
                    .OrderByDescending(
                        record =>
                            record.OccurredAtUnixMilliseconds)
                    .ToList();

            IReadOnlyList<Transaction> transactions =
                records
                    .Select(
                        TransactionRecordMapper.ToDomain)
                    .ToList();

            return Task.FromResult(transactions);
        }

        public Task UpdateAsync(
            Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(
                    nameof(transaction));
            }

            _database.Initialize();

            TransactionRecord record =
                TransactionRecordMapper.ToRecord(
                    transaction);

            int affectedRows =
                _database.Connection.Update(record);

            if (affectedRows == 0)
            {
                throw new KeyNotFoundException(
                    $"Transaction was not found: {transaction.Id}");
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(
            Guid transactionId)
        {
            if (transactionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Transaction ID cannot be empty.",
                    nameof(transactionId));
            }

            _database.Initialize();

            int affectedRows =
                _database.Connection.Execute(
                    "DELETE FROM Transactions WHERE Id = ?",
                    transactionId.ToString());

            if (affectedRows == 0)
            {
                throw new KeyNotFoundException(
                    $"Transaction was not found: {transactionId}");
            }

            return Task.CompletedTask;
        }
    }
}