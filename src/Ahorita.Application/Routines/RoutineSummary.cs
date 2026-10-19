using Ahorita.Domain.Routines;

namespace Ahorita.Application.Routines;

/// <summary>Vista de solo lectura de una rutina para listas y tarjetas.</summary>
public sealed record RoutineSummary(
    Guid Id,
    string Name,
    string Emoji,
    RoutineAccent Accent,
    int StepCount,
    TimeSpan EstimatedDuration,
    WeekDays Days,
    TimeOnly? StartTime,
    bool CanStart)
{
    public bool IsOnDemand => Days == WeekDays.None;

    /// <summary>Mismas condiciones con las que se planifican los recordatorios.</summary>
    public bool HasReminders => CanStart && !IsOnDemand && StartTime is not null;

    public static RoutineSummary From(Routine routine) => new(
        routine.Id,
        routine.Name,
        routine.Emoji,
        routine.Accent,
        routine.Steps.Count,
        routine.EstimatedDuration,
        routine.Schedule.Days,
        routine.Schedule.StartTime,
        routine.CanStart);
}
