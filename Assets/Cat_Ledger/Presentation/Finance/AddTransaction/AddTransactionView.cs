using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatLedger.Presentation.Finance.AddTransaction
{
    public sealed class AddTransactionView : MonoBehaviour
    {
        [Header("Transaction Input")]
        [SerializeField]
        private TMP_InputField amountInputField;

        [SerializeField]
        private TMP_Dropdown transactionTypeDropdown;

        [SerializeField]
        private TMP_Dropdown categoryDropdown;

        [SerializeField]
        private TMP_Dropdown paymentMethodDropdown;

        [SerializeField]
        private TMP_InputField dateInputField;

        [SerializeField]
        private TMP_InputField timeInputField;

        [SerializeField]
        private TMP_InputField noteInputField;

        [Header("Actions")]
        [SerializeField]
        private Button saveButton;

        [Header("Feedback")]
        [SerializeField]
        private TMP_Text resultText;

        public event Action SaveRequested;

        public string AmountText => amountInputField.text;

        public int SelectedTransactionTypeIndex =>
            transactionTypeDropdown.value;

        public int SelectedCategoryIndex =>
            categoryDropdown.value;

        public int SelectedPaymentMethodIndex =>
            paymentMethodDropdown.value;

        public string DateText => dateInputField.text;

        public string TimeText => timeInputField.text;

        public string NoteText => noteInputField.text;

        private void Awake()
        {
            saveButton.onClick.AddListener(
                NotifySaveRequested);
        }

        private void OnDestroy()
        {
            saveButton.onClick.RemoveListener(
                NotifySaveRequested);
        }

        public void SetDate(string dateText)
        {
            dateInputField.text = dateText;
        }

        public void SetTime(string timeText)
        {
            timeInputField.text = timeText;
        }

        public void SetSaveButtonInteractable(
            bool isInteractable)
        {
            saveButton.interactable = isInteractable;
        }

        public void ShowSuccess(string message)
        {
            resultText.text = message;
        }

        public void ShowError(string message)
        {
            resultText.text = message;
        }

        public void ClearEditableFields()
        {
            amountInputField.text = string.Empty;
            noteInputField.text = string.Empty;
        }

        private void NotifySaveRequested()
        {
            SaveRequested?.Invoke();
        }
    }
}