using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatLedger.Presentation.Common.Dialogs
{
    public sealed class ConfirmationDialogView : MonoBehaviour
    {
        [SerializeField]
        private GameObject dialogRoot;

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text messageText;

        [SerializeField]
        private TMP_Text confirmButtonText;

        [SerializeField]
        private TMP_Text cancelButtonText;

        [SerializeField]
        private Button confirmButton;

        [SerializeField]
        private Button cancelButton;

        public event Action ConfirmRequested;
        public event Action CancelRequested;

        private void Awake()
        {
            confirmButton.onClick.AddListener(
                NotifyConfirmRequested);

            cancelButton.onClick.AddListener(
                NotifyCancelRequested);

            Hide();
        }

        private void OnDestroy()
        {
            confirmButton.onClick.RemoveListener(
                NotifyConfirmRequested);

            cancelButton.onClick.RemoveListener(
                NotifyCancelRequested);
        }

        public void Show(
            string title,
            string message,
            string confirmText,
            string cancelText)
        {
            titleText.text = title;
            messageText.text = message;
            confirmButtonText.text = confirmText;
            cancelButtonText.text = cancelText;

            dialogRoot.SetActive(true);
        }

        public void Hide()
        {
            dialogRoot.SetActive(false);
        }

        public void SetButtonsInteractable(
            bool isInteractable)
        {
            confirmButton.interactable = isInteractable;
            cancelButton.interactable = isInteractable;
        }

        private void NotifyConfirmRequested()
        {
            ConfirmRequested?.Invoke();
        }

        private void NotifyCancelRequested()
        {
            CancelRequested?.Invoke();
        }
    }
}