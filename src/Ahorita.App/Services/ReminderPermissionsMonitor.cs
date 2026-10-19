using Ahorita.Application.Abstractions;
using Ahorita.Application.Reminders;

namespace Ahorita.App.Services;

/// <summary>
/// Revisa si faltan permisos para que los recordatorios suenen. Si la persona los concede
/// desde los ajustes del sistema, reprograma los avisos al volver a la app.
/// </summary>
public sealed class ReminderPermissionsMonitor(IDevicePermissions permissions, IUseCaseExecutor useCases)
{
    private MissingPermissions? _last;

    /// <summary>Lo que falta ahora mismo, sin importar si hay rutinas que avisen.</summary>
    public async Task<MissingPermissions> CheckAsync()
    {
        var missing = MissingPermissions.From(await permissions.HasNotificationsAsync(), permissions.HasExactAlarms);
        var previous = _last;
        _last = missing;

        if (previous is { } before && missing.IsResolvedFrom(before))
            await useCases.ExecuteAsync((RefreshReminders useCase) => useCase.ExecuteAsync());

        return missing;
    }

    public void OpenNotificationSettings() => permissions.OpenNotificationSettings();

    public void OpenExactAlarmSettings() => permissions.OpenExactAlarmSettings();
}
