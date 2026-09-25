using System;
using System.Threading.Tasks;
using CatLedger.Application.Common;
using CatLedger.Domain.Finance;

namespace CatLedger.Application.Finance
{
    public sealed class AddTransactionUseCase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IClock _clock;

        public AddTransactionUseCase(
            ITransactionRepository transactionRepository,
            IClock clock)
        {
            _transactionRepository = transactionRepository
                ?? throw new ArgumentNullException(nameof(transactionRepository));

            _clock = clock
                ?? throw new ArgumentNullException(nameof(clock));
        }

        public async Task<Transaction> ExecuteAsync(
            AddTransactionRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            DateTimeOffset currentTime = _clock.Now;

            var transaction = new Transaction(
                Guid.NewGuid(),
                request.AmountInMinorUnits,
                request.Type,
                request.Category,
                request.PaymentMethod,
                request.OccurredAt,
                request.Note,
                currentTime,
                currentTime);

            await _transactionRepository.AddAsync(transaction);

            return transaction;
        }
    }
}