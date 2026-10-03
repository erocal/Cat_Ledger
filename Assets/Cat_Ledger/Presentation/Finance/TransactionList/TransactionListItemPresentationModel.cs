using System;

namespace CatLedger.Presentation.Finance.TransactionList
{
    public sealed class TransactionListItemPresentationModel
    {
        public Guid TransactionId { get; }

        public string AmountText { get; }

        public string CategoryText { get; }

        public string PaymentMethodText { get; }

        public string OccurredAtText { get; }

        public string NoteText { get; }

        public TransactionListItemPresentationModel(
            Guid transactionId,
            string amountText,
            string categoryText,
            string paymentMethodText,
            string occurredAtText,
            string noteText)
        {
            TransactionId = transactionId;
            AmountText = amountText;
            CategoryText = categoryText;
            PaymentMethodText = paymentMethodText;
            OccurredAtText = occurredAtText;
            NoteText = noteText;
        }
    }
}