using Ahorita.App.Resources.Strings;

namespace Ahorita.App.Services;

/// <summary>
/// Pide permisos en el momento en que se entienden (al programar o empezar una rutina),
/// con una explicación previa. Al programar una rutina se vuelve a preguntar mientras falte
/// algo, pero nunca al abrir la app: para eso está el aviso de permisos faltantes.
/// La notificación de la rutina en curso solo se pide una vez. En Ajustes siempre se pueden volver a pedir.
/// </summary>
public sealed class PermissionPrompts(IDevicePermissions permissions, IDialogService dialogs, IPreferences preferences)
{
    private const string AskedNotificationsKey = "ahorita.asked.notifications";

    /// <summary>Para recordatorios programados: notificaciones y, si se puede, hora exacta.</summary>
    public async Task EnsureRemindersAsync()
    {
        if (!await EnsureNotificationsAsync(AppResources.Permission_NotificationsForReminders, askAgain: true))
            return;

        if (permissions.HasExactAlarms)
            return;

        var accepted = await dialogs.ConfirmAsync(
            AppResources.Permission_ExactTitle, AppResources.Permission_ExactMessage, AppResources.Permission_Allow, AppResources.Permission_NotNow);
        if (accepted)
            permissions.OpenExactAlarmSettings();
    }

    /// <summary>Para la notificación de la rutina en curso.</summary>
    public Task<bool> EnsureActiveRoutineAsync() => EnsureNotificationsAsync(AppResources.Permission_NotificationsForFocus, askAgain: false);

    private async Task<bool> EnsureNotificationsAsync(string reason, bool askAgain)
    {
        if (await permissions.HasNotificationsAsync())
            return true;

        var askedBefore = preferences.Get(AskedNotificationsKey, false);
        if (askedBefore && !askAgain)
            return false;

        preferences.Set(AskedNotificationsKey, true);
        var accepted = await dialogs.ConfirmAsync(AppResources.Permission_NotificationsTitle, reason, AppResources.Permission_Allow, AppResources.Permission_NotNow);
        if (!accepted)
            return false;

        if (await permissions.RequestNotificationsAsync())
            return true;

        // Android no vuelve a mostrar su diálogo tras rechazarlo: solo queda la pantalla de ajustes de la app.
        if (askedBefore)
            permissions.OpenNotificationSettings();

        return false;
    }
}
