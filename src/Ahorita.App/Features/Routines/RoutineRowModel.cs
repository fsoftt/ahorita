using Ahorita.App.Features.Shared;
using Ahorita.App.Resources.Strings;
using Ahorita.Application.Routines;
using Ahorita.Domain.Routines;

namespace Ahorita.App.Features.Routines;

public sealed record RoutineRowModel(Guid Id, string Name, string Emoji, RoutineAccent Accent, string Schedule, string Meta)
{
    public static RoutineRowModel From(RoutineSummary routine) => new(
        routine.Id,
        routine.Name,
        routine.Emoji,
        routine.Accent,
        RoutineText.Schedule(routine.Days, routine.StartTime),
        routine.CanStart ? RoutineText.Meta(routine.StepCount, routine.EstimatedDuration) : AppResources.Routine_Draft);
}
