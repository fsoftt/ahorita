using Ahorita.App.Droid.Reminders;
using Ahorita.App.Services;
using Android.Content;
using Android.Provider;
using AndroidX.Core.App;

namespace Ahorita.App.Droid;

internal sealed class AndroidDevicePermissions : IDevicePermissions
{
    public bool HasExactAlarms => AndroidReminderScheduler.CanScheduleExact(global::Android.App.Application.Context);

    // En Android 8–12 no hay permiso que pedir, pero la persona puede apagar las
    // notificaciones de la app: AreNotificationsEnabled refleja ambos casos.
    public Task<bool> HasNotificationsAsync() => Task.FromResult(AreNotificationsEnabled());

    public async Task<bool> RequestNotificationsAsync()
    {
        if (AreNotificationsEnabled())
            return true;

        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            return false;

        await MainThread.InvokeOnMainThreadAsync(Permissions.RequestAsync<Permissions.PostNotifications>);
        return AreNotificationsEnabled();
    }

    private static bool AreNotificationsEnabled() =>
        NotificationManagerCompat.From(global::Android.App.Application.Context)!.AreNotificationsEnabled();

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
