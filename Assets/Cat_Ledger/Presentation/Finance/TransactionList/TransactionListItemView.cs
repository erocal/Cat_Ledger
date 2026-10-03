using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatLedger.Presentation.Finance.TransactionList
{
    public sealed class TransactionListItemView : MonoBehaviour
    {
        [Header("Selection")]
        [SerializeField]
        private Button transactionContentButton;

        [Header("Transaction Information")]
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

        [Header("Actions")]
        [SerializeField]
        private GameObject actionButtonsRoot;

        [SerializeField]
        private Button editButton;

        [SerializeField]
        private Button deleteButton;

        private Guid _transactionId;

        public event Action<TransactionListItemView> ExpansionRequested;
        public event Action<Guid> EditRequested;
        public event Action<Guid> DeleteRequested;

        private void Awake()
        {
            transactionContentButton.onClick.AddListener(
                NotifyExpansionRequested);

            editButton.onClick.AddListener(
                NotifyEditRequested);

            deleteButton.onClick.AddListener(
                NotifyDeleteRequested);

            SetActionsVisible(false);
        }

        private void OnDestroy()
        {
            transactionContentButton.onClick.RemoveListener(
                NotifyExpansionRequested);

            editButton.onClick.RemoveListener(
                NotifyEditRequested);

            deleteButton.onClick.RemoveListener(
                NotifyDeleteRequested);
        }

        public void Bind(
            TransactionListItemPresentationModel presentationModel)
        {
            if (presentationModel == null)
            {
                throw new ArgumentNullException(
                    nameof(presentationModel));
            }

            _transactionId =
                presentationModel.TransactionId;

            amountText.text =
                presentationModel.AmountText;

            categoryText.text =
                presentationModel.CategoryText;

            paymentMethodText.text =
                presentationModel.PaymentMethodText;

            occurredAtText.text =
                presentationModel.OccurredAtText;

            noteText.text =
                presentationModel.NoteText;
        }

        public void SetActionsVisible(bool isVisible)
        {
            actionButtonsRoot.SetActive(isVisible);
        }

        private void NotifyExpansionRequested()
        {
            ExpansionRequested?.Invoke(this);
        }

        private void NotifyEditRequested()
        {
            EditRequested?.Invoke(_transactionId);
        }

        private void NotifyDeleteRequested()
        {
            DeleteRequested?.Invoke(_transactionId);
        }
    }
}