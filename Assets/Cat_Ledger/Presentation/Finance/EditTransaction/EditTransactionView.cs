using System;
using CatLedger.Presentation.Finance.TransactionForm;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatLedger.Presentation.Finance.EditTransaction
{
    public sealed class EditTransactionView : MonoBehaviour
    {
        [SerializeField]
        private TransactionFormView transactionFormView;

        [SerializeField]
        private Button saveButton;

        [SerializeField]
        private Button cancelButton;

        [SerializeField]
        private TMP_Text resultText;

        public TransactionFormView TransactionForm => transactionFormView;

        public event Action SaveRequested;
        public event Action CancelRequested;

        private void Awake()
        {
            saveButton.onClick.AddListener(
                NotifySaveRequested);

            cancelButton.onClick.AddListener(
                NotifyCancelRequested);
        }

        private void OnDestroy()
        {
            saveButton.onClick.RemoveListener(
                NotifySaveRequested);

            cancelButton.onClick.RemoveListener(
                NotifyCancelRequested);
        }

        public void SetSaveButtonInteractable(
            bool isInteractable)
        {
            saveButton.interactable =
                isInteractable;
        }

        public void ShowError(string message)
        {
            resultText.text = message;
        }

        public void ClearMessage()
        {
            resultText.text =
                string.Empty;
        }

        private void NotifySaveRequested()
        {
            SaveRequested?.Invoke();
        }

        private void NotifyCancelRequested()
        {
            CancelRequested?.Invoke();
        }
    }
}