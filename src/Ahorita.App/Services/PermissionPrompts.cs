using Ahorita.App.Resources.Strings;

namespace Ahorita.App.Services;

/// <summary>
/// Pide permisos en el momento en que se entienden (al programar o empezar una rutina),
/// con una explicación previa y una sola vez de forma automática: sin insistir.
/// En Ajustes siempre se pueden volver a pedir.
/// </summary>
public sealed class PermissionPrompts(IDevicePermissions permissions, IDialogService dialogs, IPreferences preferences)
{
    private const string AskedNotificationsKey = "ahorita.asked.notifications";
    private const string AskedExactAlarmsKey = "ahorita.asked.exactAlarms";

    /// <summary>Para recordatorios programados: notificaciones y, si se puede, hora exacta.</summary>
    public async Task EnsureRemindersAsync()
    {
        if (!await EnsureNotificationsAsync(AppResources.Permission_NotificationsForReminders))
            return;

        if (permissions.HasExactAlarms || preferences.Get(AskedExactAlarmsKey, false))
            return;

        preferences.Set(AskedExactAlarmsKey, true);
        var accepted = await dialogs.ConfirmAsync(
            AppResources.Permission_ExactTitle, AppResources.Permission_ExactMessage, AppResources.Permission_Allow, AppResources.Permission_NotNow);
        if (accepted)
            permissions.OpenExactAlarmSettings();
    }

    /// <summary>Para la notificación de la rutina en curso.</summary>
    public Task<bool> EnsureActiveRoutineAsync() => EnsureNotificationsAsync(AppResources.Permission_NotificationsForFocus);

    private async Task<bool> EnsureNotificationsAsync(string reason)
    {
        if (await permissions.HasNotificationsAsync())
            return true;

        if (preferences.Get(AskedNotificationsKey, false))
            return false;

        preferences.Set(AskedNotificationsKey, true);
        var accepted = await dialogs.ConfirmAsync(AppResources.Permission_NotificationsTitle, reason, AppResources.Permission_Allow, AppResources.Permission_NotNow);
        return accepted && await permissions.RequestNotificationsAsync();
    }
}
