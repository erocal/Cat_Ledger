using System;
using System.Threading.Tasks;
using CatLedger.Application.Common;
using CatLedger.Domain.Finance;

namespace CatLedger.Application.Finance
{
    public sealed class UpdateTransactionUseCase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IClock _clock;

        public UpdateTransactionUseCase(
            ITransactionRepository transactionRepository,
            IClock clock)
        {
            _transactionRepository = transactionRepository
                ?? throw new ArgumentNullException(
                    nameof(transactionRepository));

            _clock = clock
                ?? throw new ArgumentNullException(nameof(clock));
        }

        public async Task<Transaction> ExecuteAsync(
            UpdateTransactionRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            Transaction transaction =
                await _transactionRepository.GetByIdAsync(
                    request.TransactionId);

            TransactionDetails details = request.Details;

            transaction.UpdateDetails(
                details.AmountInMinorUnits,
                details.Type,
                details.Category,
                details.PaymentMethod,
                details.OccurredAt,
                details.Note,
                _clock.Now);

            await _transactionRepository.UpdateAsync(transaction);

            return transaction;
        }
    }
}