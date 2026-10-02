using System;
using UnityEngine;
using UnityEngine.UI;

namespace CatLedger.Presentation.Navigation
{
    public sealed class AppNavigator : MonoBehaviour
    {
        [Header("Pages")]
        [SerializeField]
        private GameObject ledgerPage;

        [SerializeField]
        private GameObject addTransactionPage;

        [Header("Navigation")]
        [SerializeField]
        private Button ledgerButton;

        [SerializeField]
        private Button addTransactionButton;

        [Header("Initial Page")]
        [SerializeField]
        private AppPage initialPage = AppPage.Ledger;

        public event Action<AppPage> PageChanged;

        public AppPage CurrentPage { get; private set; }

        private void Awake()
        {
            ledgerButton.onClick.AddListener(ShowLedgerPage);

            addTransactionButton.onClick.AddListener(ShowAddTransactionPage);

            NavigateTo(initialPage);
        }

        private void OnDestroy()
        {
            ledgerButton.onClick.RemoveListener(ShowLedgerPage);

            addTransactionButton.onClick.RemoveListener(ShowAddTransactionPage);
        }

        public void NavigateTo(AppPage page)
        {
            bool isLedgerVisible = page == AppPage.Ledger;

            bool isAddTransactionVisible = page == AppPage.AddTransaction;

            ledgerPage.SetActive(isLedgerVisible);
            addTransactionPage.SetActive(isAddTransactionVisible);

            CurrentPage = page;

            PageChanged?.Invoke(page);
        }

        private void ShowLedgerPage()
        {
            if (CurrentPage == AppPage.Ledger)
            {
                return;
            }

            NavigateTo(AppPage.Ledger);
        }

        private void ShowAddTransactionPage()
        {
            if (CurrentPage == AppPage.AddTransaction)
            {
                return;
            }

            NavigateTo(AppPage.AddTransaction);
        }
    }
}