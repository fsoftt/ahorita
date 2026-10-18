using Ahorita.Application.Abstractions;
using Ahorita.Domain.Progress;
using Ahorita.Domain.Sessions;

namespace Ahorita.Application.Progress;

/// <summary>Historial amable: constancia, mapa de las últimas semanas y sugerencias de tiempo.</summary>
public sealed class GetProgressSummary(ISessionRepository sessions, TimeProvider time)
{
    public const int HeatmapDays = 28;
    public const int MaxInsights = 2;

    public async Task<ProgressSummary> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var zone = time.LocalTimeZone;
        var localNow = TimeZoneInfo.ConvertTime(time.GetUtcNow(), zone);
        var today = DateOnly.FromDateTime(localNow.DateTime);
        var firstDay = today.AddDays(-(HeatmapDays - 1));
        var from = StartOfDay(firstDay, zone);
        var to = StartOfDay(today.AddDays(1), zone);

        var recent = await sessions.ListStartedBetweenAsync(from, to, cancellationToken).ConfigureAwait(false);
        var withProgress = recent.Where(s => s.DoneSteps > 0).ToArray();

        var stepsByDay = withProgress
            .GroupBy(s => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(s.StartedAt, zone).DateTime))
            .ToDictionary(g => g.Key, g => g.Sum(s => s.DoneSteps));

        var days = Enumerable.Range(0, HeatmapDays)
            .Select(i => firstDay.AddDays(i))
            .Select(d => new DayActivity(d, stepsByDay.GetValueOrDefault(d)))
            .ToArray();

        var weekStart = today.AddDays(-6);
        var thisWeek = withProgress.Where(s => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(s.StartedAt, zone).DateTime) >= weekStart).ToArray();

        return new ProgressSummary(
            ConsistencyScore.From(stepsByDay.Keys, today),
            days,
            RoutinesCompleted: thisWeek.Count(s => s.Status == SessionStatus.Completed),
            StepsDone: thisWeek.Sum(s => s.DoneSteps),
            MinutesInRoutines: (int)Math.Round(thisWeek.Sum(s => ((s.FinishedAt ?? s.StartedAt) - s.StartedAt).TotalMinutes)),
            Insights: FindInsights(recent));
    }

    private static DurationInsight[] FindInsights(IEnumerable<RoutineSession> sessions) =>
        sessions
            .SelectMany(session => session.Steps.Select(step => (session.RoutineId, Step: step)))
            .Where(x => x.Step is { Outcome: StepOutcome.Done, PlannedDuration: not null, ActualDuration: not null })
            .GroupBy(x => (x.RoutineId, x.Step.SourceStepId))
            .Select(g =>
            {
                var latest = g.Last().Step;
                return DurationInsight.Evaluate(
                    g.Key.RoutineId, g.Key.SourceStepId, latest.Title, latest.Emoji, latest.PlannedDuration!.Value,
                    g.Select(x => x.Step.ActualDuration!.Value).ToArray());
            })
            .OfType<DurationInsight>()
            .OrderByDescending(i => i.TypicalActual - i.Planned)
            .Take(MaxInsights)
            .ToArray();

    private static DateTimeOffset StartOfDay(DateOnly day, TimeZoneInfo zone)
    {
        var local = day.ToDateTime(TimeOnly.MinValue);
        return new DateTimeOffset(local, zone.GetUtcOffset(local));
    }
}

/// <param name="StepsDone">Pasos completados ese día (0 = día de descanso, sin juicio).</param>
public sealed record DayActivity(DateOnly Day, int StepsDone)
{
    /// <summary>Intensidad 0–3 para el mapa de actividad.</summary>
    public int Level => StepsDone switch
    {
        0 => 0,
        <= 3 => 1,
        <= 8 => 2,
        _ => 3,
    };
}

public sealed record ProgressSummary(
    ConsistencyScore Consistency,
    IReadOnlyList<DayActivity> Days,
    int RoutinesCompleted,
    int StepsDone,
    int MinutesInRoutines,
    IReadOnlyList<DurationInsight> Insights)
{
    public bool HasAnyActivity => Days.Any(d => d.StepsDone > 0);
}
