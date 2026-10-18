using Ahorita.Application.Routines;
using Ahorita.Domain.Routines;

namespace Ahorita.App.Features.Shared;

/// <summary>Lo que una tarjeta de rutina necesita mostrar, ya formateado.</summary>
public sealed record RoutineCardModel(
    Guid Id,
    string Name,
    string Emoji,
    RoutineAccent Accent,
    string Meta,
    string When,
    bool CanStart)
{
    public static RoutineCardModel From(RoutineSummary routine) => new(
        routine.Id,
        routine.Name,
        routine.Emoji,
        routine.Accent,
        RoutineText.Meta(routine.StepCount, routine.EstimatedDuration),
        routine.StartTime is { } time && !routine.IsOnDemand ? RoutineText.Time(time) : Resources.Strings.AppResources.Routine_OnDemand,
        routine.CanStart);
}
