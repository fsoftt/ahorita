namespace Ahorita.Application.Reminders;

/// <summary>
/// Qué permisos del sistema faltan para que los recordatorios suenen.
/// Sin notificaciones no hay aviso alguno; sin "Alarmas y recordatorios" el aviso puede llegar con retraso.
/// </summary>
public readonly record struct MissingPermissions(bool Notifications, bool ExactAlarms)
{
    public bool Any => Notifications || ExactAlarms;

    public static MissingPermissions From(bool hasNotifications, bool hasExactAlarms) =>
        new(!hasNotifications, !hasExactAlarms);

    /// <summary>Lo que hay que mostrar: nada si no hay ninguna rutina que avise.</summary>
    public MissingPermissions WhenNeeded(bool needed) => needed ? this : default;

    /// <summary>Verdadero si antes faltaba algo y ahora no: hay que reprogramar los avisos.</summary>
    public bool IsResolvedFrom(MissingPermissions previous) => previous.Any && !Any;
}
