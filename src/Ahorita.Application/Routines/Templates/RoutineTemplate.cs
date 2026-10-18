using Ahorita.Domain.Routines;

namespace Ahorita.Application.Routines.Templates;

public sealed record RoutineTemplate(
    string Key,
    string Name,
    string Emoji,
    RoutineAccent Accent,
    WeekDays Days,
    TimeOnly? StartTime,
    IReadOnlyList<RoutineTemplateStep> Steps)
{
    public TimeSpan EstimatedDuration => Steps.Aggregate(TimeSpan.Zero, (total, step) => total + (step.Duration ?? TimeSpan.Zero));
}

public sealed record RoutineTemplateStep(string Title, string? Emoji, TimeSpan? Duration, bool IsOptional = false);
