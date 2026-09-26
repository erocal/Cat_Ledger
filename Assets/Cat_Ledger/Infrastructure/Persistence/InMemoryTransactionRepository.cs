using CatLedger.Application.Finance;
using CatLedger.Domain.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatLedger.Infrastructure.Persistence
{
    public sealed class InMemoryTransactionRepository : ITransactionRepository
    {
        private readonly List<Transaction> _transactions = new();

        public IReadOnlyList<Transaction> Transactions => _transactions;

        public Task AddAsync(Transaction transaction)
        {
            _transactions.Add(transaction);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Transaction>> GetMatchingAsync(
            TransactionFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            IEnumerable<Transaction> filteredTransactions = _transactions;

            if (filter.StartOccurredAtInclusive.HasValue)
            {
                DateTimeOffset startTime =
                    filter.StartOccurredAtInclusive.Value;

                filteredTransactions = filteredTransactions.Where(
                    transaction => transaction.OccurredAt >= startTime);
            }

            if (filter.EndOccurredAtExclusive.HasValue)
            {
                DateTimeOffset endTime =
                    filter.EndOccurredAtExclusive.Value;

                filteredTransactions = filteredTransactions.Where(
                    transaction => transaction.OccurredAt < endTime);
            }

            if (filter.Category.HasValue)
            {
                TransactionCategory category = filter.Category.Value;

                filteredTransactions = filteredTransactions.Where(
                    transaction => transaction.Category == category);
            }

            if (filter.Type.HasValue)
            {
                TransactionType type = filter.Type.Value;

                filteredTransactions = filteredTransactions.Where(
                    transaction => transaction.Type == type);
            }

            IReadOnlyList<Transaction> result =
                filteredTransactions
                    .OrderByDescending(transaction => transaction.OccurredAt)
                    .ToList();

            return Task.FromResult(result);
        }
    }
}