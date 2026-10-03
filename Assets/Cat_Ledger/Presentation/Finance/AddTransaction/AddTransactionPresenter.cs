using System;
using CatLedger.Application.Common;
using CatLedger.Application.Finance;
using CatLedger.Presentation.Finance.TransactionForm;
using CatLedger.Presentation.Navigation;
using VContainer.Unity;

namespace CatLedger.Presentation.Finance.AddTransaction
{
    public sealed class AddTransactionPresenter :
        IStartable,
        IDisposable
    {
        private readonly AddTransactionView _view;
        private readonly AddTransactionUseCase
            _addTransactionUseCase;
        private readonly IClock _clock;
        private readonly AppNavigator _appNavigator;

        private bool _isSaving;

        public AddTransactionPresenter(
            AddTransactionView view,
            AddTransactionUseCase addTransactionUseCase,
            IClock clock,
            AppNavigator appNavigator)
        {
            _view = view
                ?? throw new ArgumentNullException(
                    nameof(view));

            _addTransactionUseCase =
                addTransactionUseCase
                ?? throw new ArgumentNullException(
                    nameof(addTransactionUseCase));

            _clock = clock
                ?? throw new ArgumentNullException(
                    nameof(clock));

            _appNavigator = appNavigator
                ?? throw new ArgumentNullException(
                    nameof(appNavigator));
        }

        public void Start()
        {
            _view.SaveRequested +=
                HandleSaveRequested;

            _appNavigator.PageChanged +=
                HandlePageChanged;

            if (_appNavigator.CurrentPage ==
                AppPage.AddTransaction)
            {
                InitializeForm();
            }
        }

        public void Dispose()
        {
            _view.SaveRequested -=
                HandleSaveRequested;

            _appNavigator.PageChanged -=
                HandlePageChanged;
        }

        private void HandlePageChanged(
            AppPage page)
        {
            if (page == AppPage.AddTransaction)
            {
                InitializeForm();
            }
        }

        private async void HandleSaveRequested()
        {
            if (_isSaving)
            {
                return;
            }

            try
            {
                _isSaving = true;

                _view.SetSaveButtonInteractable(
                    false);

                TransactionDetails transactionDetails =
                    TransactionFormMapper
                        .ToTransactionDetails(
                            _view.TransactionForm
                                .ReadFormData());

                var request =
                    new AddTransactionRequest(
                        transactionDetails);

                await _addTransactionUseCase
                    .ExecuteAsync(request);

                _view.ShowSuccess(
                    "Transaction saved.");

                _view.TransactionForm
                    .ClearAmountAndNote();
            }
            catch (Exception exception)
            {
                _view.ShowError(
                    exception.Message);
            }
            finally
            {
                _isSaving = false;

                _view.SetSaveButtonInteractable(
                    true);
            }
        }

        private void InitializeForm()
        {
            TransactionFormData defaultFormData =
                TransactionFormMapper
                    .CreateNewTransactionDefaults(
                        _clock.Now);

            _view.TransactionForm
                .DisplayFormData(
                    defaultFormData);
        }
    }
}