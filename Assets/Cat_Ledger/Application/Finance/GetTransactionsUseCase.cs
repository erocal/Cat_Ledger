using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatLedger.Domain.Finance;

namespace CatLedger.Application.Finance
{
    public sealed class GetTransactionsUseCase
    {
        private readonly ITransactionRepository _transactionRepository;

        public GetTransactionsUseCase(
            ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository
                ?? throw new ArgumentNullException(
                    nameof(transactionRepository));
        }

        public Task<IReadOnlyList<Transaction>> ExecuteAsync(
            TransactionFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            return _transactionRepository.GetMatchingAsync(filter);
        }
    }
}