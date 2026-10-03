using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CatLedger.Presentation.Finance.TransactionList
{
    public sealed class TransactionListView : MonoBehaviour
    {
        [SerializeField]
        private Transform contentRoot;

        [SerializeField]
        private TransactionListItemView itemPrefab;

        [SerializeField]
        private TMP_Text emptyStateText;

        private readonly List<TransactionListItemView>
            _itemViews = new();

        private TransactionListItemView _expandedItemView;

        public event Action<Guid> EditTransactionRequested;
        public event Action<Guid> DeleteTransactionRequested;

        public void ShowTransactions(
            IReadOnlyList<TransactionListItemPresentationModel>
                presentationModels)
        {
            if (presentationModels == null)
            {
                throw new ArgumentNullException(
                    nameof(presentationModels));
            }

            ClearItems();

            foreach (
                TransactionListItemPresentationModel presentationModel
                in presentationModels)
            {
                TransactionListItemView itemView =
                    Instantiate(
                        itemPrefab,
                        contentRoot);

                itemView.Bind(presentationModel);

                itemView.ExpansionRequested +=
                    HandleExpansionRequested;

                itemView.EditRequested +=
                    HandleEditRequested;

                itemView.DeleteRequested +=
                    HandleDeleteRequested;

                _itemViews.Add(itemView);
            }

            SetEmptyStateVisible(
                presentationModels.Count == 0);
        }

        public void ShowError(string message)
        {
            Debug.LogError(
                $"Failed to display transactions: {message}");
        }

        private void HandleExpansionRequested(
            TransactionListItemView requestedItemView)
        {
            if (_expandedItemView == requestedItemView)
            {
                _expandedItemView.SetActionsVisible(false);
                _expandedItemView = null;

                return;
            }

            if (_expandedItemView != null)
            {
                _expandedItemView.SetActionsVisible(false);
            }

            requestedItemView.SetActionsVisible(true);

            _expandedItemView = requestedItemView;
        }

        private void HandleEditRequested(
            Guid transactionId)
        {
            EditTransactionRequested?.Invoke(
                transactionId);
        }

        private void HandleDeleteRequested(
            Guid transactionId)
        {
            DeleteTransactionRequested?.Invoke(
                transactionId);
        }

        private void ClearItems()
        {
            _expandedItemView = null;

            foreach (
                TransactionListItemView itemView
                in _itemViews)
            {
                if (itemView == null)
                {
                    continue;
                }

                itemView.ExpansionRequested -=
                    HandleExpansionRequested;

                itemView.EditRequested -=
                    HandleEditRequested;

                itemView.DeleteRequested -=
                    HandleDeleteRequested;

                Destroy(itemView.gameObject);
            }

            _itemViews.Clear();
        }

        private void SetEmptyStateVisible(
            bool isVisible)
        {
            if (emptyStateText != null)
            {
                emptyStateText.gameObject.SetActive(
                    isVisible);
            }
        }
    }
}