using Ahorita.Domain.Routines;

namespace Ahorita.Application.Tests.Fakes;

internal static class Routines
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

    public static Routine Scheduled(string name, WeekDays days, int hour, int minute = 0, int stepMinutes = 5, int steps = 1)
    {
        var schedule = RoutineSchedule.Create(days, new TimeOnly(hour, minute)).Value;
        return WithSteps(Routine.Create(name, "⭐", RoutineAccent.Mint, schedule, CreatedAt).Value, steps, stepMinutes);
    }

    public static Routine OnDemand(string name, int steps = 1) =>
        WithSteps(Routine.Create(name, "⭐", RoutineAccent.Sky, RoutineSchedule.OnDemand, CreatedAt).Value, steps, stepMinutes: 1);

    private static Routine WithSteps(Routine routine, int steps, int stepMinutes)
    {
        for (var i = 0; i < steps; i++)
            routine.AddStep($"Paso {i + 1}", duration: TimeSpan.FromMinutes(stepMinutes));

        return routine;
    }
}
