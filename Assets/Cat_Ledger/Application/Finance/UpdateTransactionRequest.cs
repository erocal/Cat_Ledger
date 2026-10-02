using System;

namespace CatLedger.Application.Finance
{
    public sealed class UpdateTransactionRequest
    {
        public Guid TransactionId { get; }

        public TransactionDetails Details { get; }

        public UpdateTransactionRequest(
            Guid transactionId,
            TransactionDetails details)
        {
            if (transactionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Transaction ID cannot be empty.",
                    nameof(transactionId));
            }

            TransactionId = transactionId;

            Details = details
                ?? throw new ArgumentNullException(nameof(details));
        }
    }
}