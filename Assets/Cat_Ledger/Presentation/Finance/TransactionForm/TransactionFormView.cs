using System;
using TMPro;
using UnityEngine;

namespace CatLedger.Presentation.Finance.TransactionForm
{
    public sealed class TransactionFormView : MonoBehaviour
    {
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

        public TransactionFormData ReadFormData()
        {
            return new TransactionFormData(
                amountInputField.text,
                transactionTypeDropdown.value,
                categoryDropdown.value,
                paymentMethodDropdown.value,
                dateInputField.text,
                timeInputField.text,
                noteInputField.text);
        }

        public void DisplayFormData(
            TransactionFormData formData)
        {
            if (formData == null)
            {
                throw new ArgumentNullException(
                    nameof(formData));
            }

            amountInputField.text =
                formData.AmountText;

            transactionTypeDropdown.value =
                formData.TransactionTypeValue;

            categoryDropdown.value =
                formData.CategoryValue;

            paymentMethodDropdown.value =
                formData.PaymentMethodValue;

            dateInputField.text =
                formData.DateText;

            timeInputField.text =
                formData.TimeText;

            noteInputField.text =
                formData.NoteText;
        }

        public void ClearAmountAndNote()
        {
            amountInputField.text =
                string.Empty;

            noteInputField.text =
                string.Empty;
        }
    }
}