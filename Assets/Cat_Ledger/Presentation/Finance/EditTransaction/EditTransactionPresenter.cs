using System;
using CatLedger.Application.Finance;
using CatLedger.Domain.Finance;
using CatLedger.Presentation.Finance.TransactionForm;
using CatLedger.Presentation.Navigation;
using VContainer.Unity;

namespace CatLedger.Presentation.Finance.EditTransaction
{
    public sealed class EditTransactionPresenter :
        IStartable,
        IDisposable
    {
        private readonly EditTransactionView _view;
        private readonly GetTransactionByIdUseCase
            _getTransactionByIdUseCase;
        private readonly UpdateTransactionUseCase
            _updateTransactionUseCase;
        private readonly EditTransactionNavigationState
            _navigationState;
        private readonly AppNavigator _appNavigator;

        private Guid _editingTransactionId =
            Guid.Empty;

        private bool _isBusy;

        public EditTransactionPresenter(
            EditTransactionView view,
            GetTransactionByIdUseCase
                getTransactionByIdUseCase,
            UpdateTransactionUseCase
                updateTransactionUseCase,
            EditTransactionNavigationState
                navigationState,
            AppNavigator appNavigator)
        {
            _view = view
                ?? throw new ArgumentNullException(
                    nameof(view));

            _getTransactionByIdUseCase =
                getTransactionByIdUseCase
                ?? throw new ArgumentNullException(
                    nameof(getTransactionByIdUseCase));

            _updateTransactionUseCase =
                updateTransactionUseCase
                ?? throw new ArgumentNullException(
                    nameof(updateTransactionUseCase));

            _navigationState =
                navigationState
                ?? throw new ArgumentNullException(
                    nameof(navigationState));

            _appNavigator =
                appNavigator
                ?? throw new ArgumentNullException(
                    nameof(appNavigator));
        }

        public void Start()
        {
            _view.SaveRequested +=
                HandleSaveRequested;

            _view.CancelRequested +=
                HandleCancelRequested;

            _appNavigator.PageChanged +=
                HandlePageChanged;

            if (_appNavigator.CurrentPage ==
                AppPage.EditTransaction)
            {
                LoadSelectedTransaction();
            }
        }

        public void Dispose()
        {
            _view.SaveRequested -=
                HandleSaveRequested;

            _view.CancelRequested -=
                HandleCancelRequested;

            _appNavigator.PageChanged -=
                HandlePageChanged;
        }

        private void HandlePageChanged(
            AppPage page)
        {
            if (page == AppPage.EditTransaction)
            {
                LoadSelectedTransaction();
            }
        }

        private async void LoadSelectedTransaction()
        {
            if (_isBusy)
            {
                return;
            }

            if (!_navigationState
                .TryGetSelectedTransactionId(
                    out Guid transactionId))
            {
                _view.ShowError(
                    "No transaction was selected.");

                return;
            }

            try
            {
                _isBusy = true;

                _view.ClearMessage();

                Transaction transaction =
                    await _getTransactionByIdUseCase
                        .ExecuteAsync(transactionId);

                if (_appNavigator.CurrentPage !=
                    AppPage.EditTransaction)
                {
                    return;
                }

                _editingTransactionId =
                    transaction.Id;

                TransactionFormData formData =
                    TransactionFormMapper
                        .FromTransaction(transaction);

                _view.TransactionForm
                    .DisplayFormData(formData);
            }
            catch (Exception exception)
            {
                _view.ShowError(
                    exception.Message);
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async void HandleSaveRequested()
        {
            if (_isBusy ||
                _editingTransactionId == Guid.Empty)
            {
                return;
            }

            try
            {
                _isBusy = true;

                _view.SetSaveButtonInteractable(
                    false);

                TransactionDetails transactionDetails =
                    TransactionFormMapper
                        .ToTransactionDetails(
                            _view.TransactionForm
                                .ReadFormData());

                var request =
                    new UpdateTransactionRequest(
                        _editingTransactionId,
                        transactionDetails);

                await _updateTransactionUseCase
                    .ExecuteAsync(request);

                LeaveEditPage();
            }
            catch (Exception exception)
            {
                _view.ShowError(
                    exception.Message);
            }
            finally
            {
                _isBusy = false;

                _view.SetSaveButtonInteractable(
                    true);
            }
        }

        private void HandleCancelRequested()
        {
            if (_isBusy)
            {
                return;
            }

            LeaveEditPage();
        }

        private void LeaveEditPage()
        {
            _editingTransactionId =
                Guid.Empty;

            _navigationState.Clear();

            _appNavigator.NavigateTo(
                AppPage.Ledger);
        }
    }
}