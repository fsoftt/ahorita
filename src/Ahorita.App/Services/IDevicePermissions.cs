namespace Ahorita.App.Services;

/// <summary>Permisos del sistema que Ahorita pide siempre en contexto, nunca al abrir.</summary>
public interface IDevicePermissions
{
    Task<bool> HasNotificationsAsync();

    Task<bool> RequestNotificationsAsync();

    /// <summary>"Alarmas y recordatorios" (Android 12+): avisos a la hora exacta.</summary>
    bool HasExactAlarms { get; }

    /// <summary>Abre los ajustes de notificaciones de la app: lo único que queda si se rechazaron del todo.</summary>
    void OpenNotificationSettings();

    /// <summary>Abre la pantalla del sistema para conceder alarmas exactas.</summary>
    void OpenExactAlarmSettings();
}
