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

        public Task<Transaction> GetByIdAsync(Guid transactionId)
        {
            Transaction transaction =
                _transactions.FirstOrDefault(
                    transaction => transaction.Id == transactionId);

            if (transaction == null)
            {
                throw new KeyNotFoundException(
                    $"Transaction was not found: {transactionId}");
            }

            return Task.FromResult(transaction);
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

        public Task UpdateAsync(Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            int transactionIndex =
                _transactions.FindIndex(
                    existingTransaction =>
                        existingTransaction.Id == transaction.Id);

            if (transactionIndex < 0)
            {
                throw new KeyNotFoundException(
                    $"Transaction was not found: {transaction.Id}");
            }

            _transactions[transactionIndex] = transaction;

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid transactionId)
        {
            int removedCount =
                _transactions.RemoveAll(
                    transaction => transaction.Id == transactionId);

            if (removedCount == 0)
            {
                throw new KeyNotFoundException(
                    $"Transaction was not found: {transactionId}");
            }

            return Task.CompletedTask;
        }
    }
}