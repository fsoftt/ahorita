namespace Ahorita.App.Services;

internal sealed class DialogService : IDialogService
{
    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel) =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlertAsync(title, message, accept, cancel));
}
