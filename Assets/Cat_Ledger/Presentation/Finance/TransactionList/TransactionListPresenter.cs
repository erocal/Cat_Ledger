using CatLedger.Application.Finance;
using CatLedger.Domain.Finance;
using CatLedger.Presentation.Finance.EditTransaction;
using CatLedger.Presentation.Navigation;
using System;
using System.Collections.Generic;
using System.Globalization;
using VContainer.Unity;
using System.Threading.Tasks;
using CatLedger.Presentation.Common.Dialogs;

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
        private readonly EditTransactionNavigationState
            _editTransactionNavigationState;

        private readonly DeleteTransactionUseCase _deleteTransactionUseCase;

        private readonly ConfirmationDialogController _confirmationDialogController;

        private bool _isDeleting;

        private bool _isRefreshing;

        public TransactionListPresenter(
            TransactionListView view,
            GetTransactionsUseCase getTransactionsUseCase,
            AppNavigator appNavigator,
            EditTransactionNavigationState editTransactionNavigationState,
            DeleteTransactionUseCase deleteTransactionUseCase,
            ConfirmationDialogController confirmationDialogController)
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

            _editTransactionNavigationState = editTransactionNavigationState
                ?? throw new ArgumentNullException(
                    nameof(editTransactionNavigationState));

            _deleteTransactionUseCase = deleteTransactionUseCase
                ?? throw new ArgumentNullException(
                    nameof(deleteTransactionUseCase));

            _confirmationDialogController = confirmationDialogController
                ?? throw new ArgumentNullException(
                    nameof(confirmationDialogController));
        }

        public void Start()
        {
            _appNavigator.PageChanged +=
                HandlePageChanged;

            _view.EditTransactionRequested +=
                HandleEditTransactionRequested;

            _view.DeleteTransactionRequested +=
                HandleDeleteTransactionRequested;

            if (_appNavigator.CurrentPage ==
                AppPage.Ledger)
            {
                _ = RefreshAsync();
            }
        }

        public void Dispose()
        {
            _appNavigator.PageChanged -=
                HandlePageChanged;

            _view.EditTransactionRequested -=
                HandleEditTransactionRequested;

            _view.DeleteTransactionRequested -=
                HandleDeleteTransactionRequested;
        }

        private void HandlePageChanged(AppPage page)
        {
            if (page == AppPage.Ledger)
            {
                _ = RefreshAsync();
            }
        }

        private void HandleEditTransactionRequested(Guid transactionId)
        {
            _editTransactionNavigationState.SelectTransaction(transactionId);

            _appNavigator.NavigateTo(AppPage.EditTransaction);
        }

        private async void HandleDeleteTransactionRequested(Guid transactionId)
        {
            if (_isDeleting ||
                transactionId == Guid.Empty)
            {
                return;
            }

            try
            {
                bool confirmed =
                    await _confirmationDialogController.ShowAsync(
                        title: "Delete Transaction",
                        message:
                            "Are you sure you want to delete this transaction?",
                        confirmText: "Delete",
                        cancelText: "Cancel");

                if (!confirmed)
                {
                    return;
                }

                _isDeleting = true;

                await _deleteTransactionUseCase
                    .ExecuteAsync(transactionId);

                if (_appNavigator.CurrentPage ==
                    AppPage.Ledger)
                {
                    await RefreshAsync();
                }
            }
            catch (Exception exception)
            {
                _view.ShowError(
                    exception.Message);
            }
            finally
            {
                _isDeleting = false;
            }
        }

        private async Task RefreshAsync()
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
                    await _getTransactionsUseCase
                        .ExecuteAsync(filter);

                if (_appNavigator.CurrentPage !=
                    AppPage.Ledger)
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
                transaction.Id,
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