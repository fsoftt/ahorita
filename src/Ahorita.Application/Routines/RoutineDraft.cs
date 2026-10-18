using Ahorita.Domain.Routines;

namespace Ahorita.Application.Routines;

/// <summary>
/// Estado editable de una rutina completa. El editor trabaja sobre esta copia y la
/// guarda de una vez con <see cref="SaveRoutine"/>.
/// </summary>
/// <param name="Id">Null para una rutina nueva.</param>
public sealed record RoutineDraft(
    Guid? Id,
    string Name,
    string Emoji,
    RoutineAccent Accent,
    WeekDays Days,
    TimeOnly? StartTime,
    TimeSpan? ReminderLeadTime,
    IReadOnlyList<StepDraft> Steps)
{
    public static RoutineDraft From(Routine routine) => new(
        routine.Id,
        routine.Name,
        routine.Emoji,
        routine.Accent,
        routine.Schedule.Days,
        routine.Schedule.StartTime,
        routine.Schedule.IsOnDemand ? null : routine.Schedule.ReminderLeadTime,
        routine.Steps.Select(s => new StepDraft(s.Id, s.Title, s.Emoji, s.Duration, s.IsOptional)).ToArray());
}

/// <param name="Id">Null para un paso nuevo.</param>
public sealed record StepDraft(Guid? Id, string Title, string? Emoji, TimeSpan? Duration, bool IsOptional);
