namespace Ahorita.Application.Reminders;

public enum ReminderKind
{
    /// <summary>Aviso previo: "En 10 min empieza…".</summary>
    HeadsUp,

    /// <summary>Hora de empezar, con acciones Empezar / En 5 min / Hoy no.</summary>
    Start,
}

/// <param name="FireAt">Cuándo se muestra el aviso.</param>
/// <param name="StartsAt">Cuándo empieza la rutina (igual a <paramref name="FireAt"/> salvo en el aviso previo).</param>
public sealed record ScheduledReminder(
    Guid RoutineId,
    ReminderKind Kind,
    DateTimeOffset FireAt,
    DateTimeOffset StartsAt,
    string RoutineName,
    string RoutineEmoji);
