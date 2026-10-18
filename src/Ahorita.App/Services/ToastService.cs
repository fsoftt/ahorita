using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace Ahorita.App.Services;

internal sealed class ToastService : IToastService
{
    public Task ShowAsync(string message) =>
        MainThread.InvokeOnMainThreadAsync(() => Toast.Make(message, ToastDuration.Short).Show());
}
