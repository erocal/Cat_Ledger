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
    public sealed class SQLiteTransactionRepository : ITransactionRepository
    {
        private readonly CatLedgerDatabase _database;

        public SQLiteTransactionRepository(CatLedgerDatabase database)
        {
            _database = database
                ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task AddAsync(Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            await _database.InitializeAsync();

            TransactionRecord record =
                TransactionRecordMapper.ToRecord(transaction);

            await _database.Connection.InsertAsync(record);
        }

        public async Task<IReadOnlyList<Transaction>> GetMatchingAsync(
            TransactionFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            await _database.InitializeAsync();

            AsyncTableQuery<TransactionRecord> databaseQuery =
                _database.Connection.Table<TransactionRecord>();

            if (filter.StartOccurredAtInclusive.HasValue)
            {
                long startUnixMilliseconds =
                    filter.StartOccurredAtInclusive.Value
                        .ToUnixTimeMilliseconds();

                databaseQuery = databaseQuery.Where(
                    record =>
                        record.OccurredAtUnixMilliseconds >=
                        startUnixMilliseconds);
            }

            if (filter.EndOccurredAtExclusive.HasValue)
            {
                long endUnixMilliseconds =
                    filter.EndOccurredAtExclusive.Value
                        .ToUnixTimeMilliseconds();

                databaseQuery = databaseQuery.Where(
                    record =>
                        record.OccurredAtUnixMilliseconds <
                        endUnixMilliseconds);
            }

            if (filter.Category.HasValue)
            {
                int categoryValue =
                    (int)filter.Category.Value;

                databaseQuery = databaseQuery.Where(
                    record => record.Category == categoryValue);
            }

            if (filter.Type.HasValue)
            {
                int transactionTypeValue =
                    (int)filter.Type.Value;

                databaseQuery = databaseQuery.Where(
                    record => record.Type == transactionTypeValue);
            }

            List<TransactionRecord> records =
                await databaseQuery
                    .OrderByDescending(
                        record => record.OccurredAtUnixMilliseconds)
                    .ToListAsync();

            IReadOnlyList<Transaction> transactions =
                records
                    .Select(TransactionRecordMapper.ToDomain)
                    .ToList();

            return transactions;
        }
    }
}