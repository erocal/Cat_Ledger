using System;
using System.Threading.Tasks;

namespace CatLedger.Presentation.Common.Dialogs
{
    public sealed class ConfirmationDialogController :
        IDisposable
    {
        private readonly ConfirmationDialogView _view;

        private TaskCompletionSource<bool>
            _pendingResult;

        public ConfirmationDialogController(
            ConfirmationDialogView view)
        {
            _view = view
                ?? throw new ArgumentNullException(
                    nameof(view));

            _view.ConfirmRequested +=
                HandleConfirmRequested;

            _view.CancelRequested +=
                HandleCancelRequested;
        }

        public Task<bool> ShowAsync(
            string title,
            string message,
            string confirmText = "Confirm",
            string cancelText = "Cancel")
        {
            if (_pendingResult != null)
            {
                throw new InvalidOperationException(
                    "A confirmation dialog is already open.");
            }

            _pendingResult =
                new TaskCompletionSource<bool>();

            _view.Show(
                title,
                message,
                confirmText,
                cancelText);

            return _pendingResult.Task;
        }

        public void Dispose()
        {
            _view.ConfirmRequested -=
                HandleConfirmRequested;

            _view.CancelRequested -=
                HandleCancelRequested;
        }

        private void HandleConfirmRequested()
        {
            Complete(true);
        }

        private void HandleCancelRequested()
        {
            Complete(false);
        }

        private void Complete(bool confirmed)
        {
            TaskCompletionSource<bool> pendingResult =
                _pendingResult;

            if (pendingResult == null)
            {
                return;
            }

            _pendingResult = null;

            _view.Hide();

            pendingResult.SetResult(confirmed);
        }
    }
}