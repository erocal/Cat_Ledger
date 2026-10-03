using System;

namespace CatLedger.Presentation.Finance.EditTransaction
{
    public sealed class EditTransactionNavigationState
    {
        private Guid _selectedTransactionId =
            Guid.Empty;

        public void SelectTransaction(
            Guid transactionId)
        {
            if (transactionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Transaction ID cannot be empty.",
                    nameof(transactionId));
            }

            _selectedTransactionId =
                transactionId;
        }

        public bool TryGetSelectedTransactionId(
            out Guid transactionId)
        {
            transactionId =
                _selectedTransactionId;

            return transactionId != Guid.Empty;
        }

        public void Clear()
        {
            _selectedTransactionId =
                Guid.Empty;
        }
    }
}