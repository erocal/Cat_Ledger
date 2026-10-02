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

        private readonly List<TransactionListItemView> _itemViews = new();

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

            foreach (TransactionListItemPresentationModel presentationModel
                     in presentationModels)
            {
                TransactionListItemView itemView =
                    Instantiate(
                        itemPrefab,
                        contentRoot);

                itemView.Bind(presentationModel);

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

        private void ClearItems()
        {
            foreach (TransactionListItemView itemView in _itemViews)
            {
                if (itemView != null)
                {
                    Destroy(itemView.gameObject);
                }
            }

            _itemViews.Clear();
        }

        private void SetEmptyStateVisible(bool isVisible)
        {
            if (emptyStateText != null)
            {
                emptyStateText.gameObject.SetActive(isVisible);
            }
        }
    }
}