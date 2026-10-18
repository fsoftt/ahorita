namespace Ahorita.Application.Reminders;

public enum ReminderKind
{
    /// <summary>Aviso previo: "En 10 min empieza…".</summary>
    HeadsUp,

    /// <summary>Hora de empezar, con acciones Empezar / En 5 min / Hoy no.</summary>
    Start,
}

public sealed record ScheduledReminder(Guid RoutineId, ReminderKind Kind, DateTimeOffset FireAt, string RoutineName, string RoutineEmoji);
