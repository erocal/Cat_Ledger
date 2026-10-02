using System;
using System.Collections.Generic;
using System.Globalization;
using CatLedger.Application.Finance;
using CatLedger.Domain.Finance;
using CatLedger.Presentation.Navigation;
using VContainer.Unity;

namespace CatLedger.Presentation.Finance.TransactionList
{
    public sealed class TransactionListPresenter :
        IStartable,
        IDisposable
    {
        private const string OccurredAtFormat =
            "yyyy-MM-dd HH:mm";

        private readonly TransactionListView _view;
        private readonly GetTransactionsUseCase
            _getTransactionsUseCase;
        private readonly AppNavigator _appNavigator;

        private bool _isRefreshing;

        public TransactionListPresenter(
            TransactionListView view,
            GetTransactionsUseCase getTransactionsUseCase,
            AppNavigator appNavigator)
        {
            _view = view
                ?? throw new ArgumentNullException(
                    nameof(view));

            _getTransactionsUseCase =
                getTransactionsUseCase
                ?? throw new ArgumentNullException(
                    nameof(getTransactionsUseCase));

            _appNavigator = appNavigator
                ?? throw new ArgumentNullException(
                    nameof(appNavigator));
        }

        public void Start()
        {
            _appNavigator.PageChanged += HandlePageChanged;

            if (_appNavigator.CurrentPage == AppPage.Ledger)
            {
                Refresh();
            }
        }

        public void Dispose()
        {
            _appNavigator.PageChanged -= HandlePageChanged;
        }

        private void HandlePageChanged(AppPage page)
        {
            if (page == AppPage.Ledger)
            {
                Refresh();
            }
        }

        private async void Refresh()
        {
            if (_isRefreshing)
            {
                return;
            }

            try
            {
                _isRefreshing = true;

                var filter =
                    new TransactionFilter();

                IReadOnlyList<Transaction> transactions =
                    await _getTransactionsUseCase.ExecuteAsync(
                        filter);

                if (_appNavigator.CurrentPage != AppPage.Ledger)
                {
                    return;
                }

                IReadOnlyList<
                    TransactionListItemPresentationModel>
                    presentationModels =
                        CreatePresentationModels(
                            transactions);

                _view.ShowTransactions(
                    presentationModels);
            }
            catch (Exception exception)
            {
                if (_appNavigator.CurrentPage ==
                    AppPage.Ledger)
                {
                    _view.ShowError(
                        exception.Message);
                }
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private static IReadOnlyList<
            TransactionListItemPresentationModel>
            CreatePresentationModels(
                IReadOnlyList<Transaction> transactions)
        {
            var presentationModels =
                new List<
                    TransactionListItemPresentationModel>(
                    transactions.Count);

            foreach (Transaction transaction
                     in transactions)
            {
                presentationModels.Add(
                    CreatePresentationModel(
                        transaction));
            }

            return presentationModels;
        }

        private static TransactionListItemPresentationModel
            CreatePresentationModel(
                Transaction transaction)
        {
            string amountText =
                FormatAmount(transaction);

            string occurredAtText =
                transaction.OccurredAt.ToString(
                    OccurredAtFormat,
                    CultureInfo.InvariantCulture);

            return new TransactionListItemPresentationModel(
                amountText,
                transaction.Category.ToString(),
                transaction.PaymentMethod.ToString(),
                occurredAtText,
                transaction.Note);
        }

        private static string FormatAmount(
            Transaction transaction)
        {
            string formattedAmount =
                transaction.AmountInMinorUnits.ToString(
                    "N0",
                    CultureInfo.InvariantCulture);

            return transaction.Type switch
            {
                TransactionType.Income =>
                    $"+{formattedAmount}",

                TransactionType.Expense =>
                    $"-{formattedAmount}",

                _ => throw new ArgumentOutOfRangeException(
                    nameof(transaction.Type),
                    transaction.Type,
                    "Unsupported transaction type.")
            };
        }
    }
}