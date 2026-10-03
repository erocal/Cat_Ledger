namespace CatLedger.Presentation.Finance.TransactionForm
{
    public sealed class TransactionFormData
    {
        public string AmountText { get; }

        public int TransactionTypeValue { get; }

        public int CategoryValue { get; }

        public int PaymentMethodValue { get; }

        public string DateText { get; }

        public string TimeText { get; }

        public string NoteText { get; }

        public TransactionFormData(
            string amountText,
            int transactionTypeValue,
            int categoryValue,
            int paymentMethodValue,
            string dateText,
            string timeText,
            string noteText)
        {
            AmountText = amountText;
            TransactionTypeValue = transactionTypeValue;
            CategoryValue = categoryValue;
            PaymentMethodValue = paymentMethodValue;
            DateText = dateText;
            TimeText = timeText;
            NoteText = noteText;
        }
    }
}