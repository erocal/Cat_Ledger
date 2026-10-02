using System;
using System.Threading.Tasks;

namespace CatLedger.Application.Finance
{
    public sealed class DeleteTransactionUseCase
    {
        private readonly ITransactionRepository _transactionRepository;

        public DeleteTransactionUseCase(
            ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository
                ?? throw new ArgumentNullException(
                    nameof(transactionRepository));
        }

        public Task ExecuteAsync(Guid transactionId)
        {
            if (transactionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Transaction ID cannot be empty.",
                    nameof(transactionId));
            }

            return _transactionRepository.DeleteAsync(transactionId);
        }
    }
}