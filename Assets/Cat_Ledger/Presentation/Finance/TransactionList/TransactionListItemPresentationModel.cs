namespace CatLedger.Presentation.Finance.TransactionList
{
    public sealed class TransactionListItemPresentationModel
    {
        public string AmountText { get; }

        public string CategoryText { get; }

        public string PaymentMethodText { get; }

        public string OccurredAtText { get; }

        public string NoteText { get; }

        public TransactionListItemPresentationModel(
            string amountText,
            string categoryText,
            string paymentMethodText,
            string occurredAtText,
            string noteText)
        {
            AmountText = amountText;
            CategoryText = categoryText;
            PaymentMethodText = paymentMethodText;
            OccurredAtText = occurredAtText;
            NoteText = noteText;
        }
    }
}