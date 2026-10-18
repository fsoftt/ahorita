using Ahorita.App.Droid.Reminders;
using Ahorita.App.Services;
using Android.Content;
using Android.Provider;

namespace Ahorita.App.Droid;

internal sealed class AndroidDevicePermissions : IDevicePermissions
{
    public bool HasExactAlarms => AndroidReminderScheduler.CanScheduleExact(global::Android.App.Application.Context);

    public async Task<bool> HasNotificationsAsync() =>
        await Permissions.CheckStatusAsync<Permissions.PostNotifications>() == PermissionStatus.Granted;

    public async Task<bool> RequestNotificationsAsync() =>
        await MainThread.InvokeOnMainThreadAsync(Permissions.RequestAsync<Permissions.PostNotifications>) == PermissionStatus.Granted;

    public void OpenExactAlarmSettings()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(31))
            return;

        var context = global::Android.App.Application.Context;
        var intent = new Intent(Settings.ActionRequestScheduleExactAlarm)
            .SetData(global::Android.Net.Uri.Parse("package:" + context.PackageName))
            .AddFlags(ActivityFlags.NewTask);
        context.StartActivity(intent);
    }
}
