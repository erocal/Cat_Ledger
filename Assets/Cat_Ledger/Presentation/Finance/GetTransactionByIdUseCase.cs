using System;
using System.Threading.Tasks;
using CatLedger.Domain.Finance;

namespace CatLedger.Application.Finance
{
    public sealed class GetTransactionByIdUseCase
    {
        private readonly ITransactionRepository
            _transactionRepository;

        public GetTransactionByIdUseCase(
            ITransactionRepository transactionRepository)
        {
            _transactionRepository =
                transactionRepository
                ?? throw new ArgumentNullException(
                    nameof(transactionRepository));
        }

        public Task<Transaction> ExecuteAsync(
            Guid transactionId)
        {
            if (transactionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Transaction ID cannot be empty.",
                    nameof(transactionId));
            }

            return _transactionRepository
                .GetByIdAsync(transactionId);
        }
    }
}