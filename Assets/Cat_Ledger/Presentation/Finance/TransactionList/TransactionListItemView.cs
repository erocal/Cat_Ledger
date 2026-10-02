using TMPro;
using UnityEngine;

namespace CatLedger.Presentation.Finance.TransactionList
{
    public sealed class TransactionListItemView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text amountText;

        [SerializeField]
        private TMP_Text categoryText;

        [SerializeField]
        private TMP_Text paymentMethodText;

        [SerializeField]
        private TMP_Text occurredAtText;

        [SerializeField]
        private TMP_Text noteText;

        public void Bind(
            TransactionListItemPresentationModel presentationModel)
        {
            amountText.text = presentationModel.AmountText;
            categoryText.text = presentationModel.CategoryText;
            paymentMethodText.text =
                presentationModel.PaymentMethodText;
            occurredAtText.text =
                presentationModel.OccurredAtText;
            noteText.text = presentationModel.NoteText;
        }
    }
}