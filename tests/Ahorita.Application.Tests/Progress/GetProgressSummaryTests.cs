using Ahorita.Application.Progress;
using Ahorita.Application.Tests.Fakes;
using Ahorita.Domain.Routines;
using Ahorita.Domain.Sessions;
using Microsoft.Extensions.Time.Testing;

namespace Ahorita.Application.Tests.Progress;

public sealed class GetProgressSummaryTests
{
    // Lunes 19 de octubre de 2026, 20:00 UTC.
    private static readonly DateTimeOffset Now = new(2026, 10, 19, 20, 0, 0, TimeSpan.Zero);

    private readonly InMemorySessionRepository _sessions = new();
    private readonly FakeTimeProvider _time = new(Now);
    private readonly Routine _routine;

    public GetProgressSummaryTests()
    {
        _time.SetLocalTimeZone(TimeZoneInfo.Utc);
        _routine = Routine.Create("Mañana", "🌅", RoutineAccent.Tangerine, RoutineSchedule.OnDemand, Now).Value;
        _routine.AddStep("Agua", duration: TimeSpan.FromMinutes(1));
        _routine.AddStep("Ducha", duration: TimeSpan.FromMinutes(10));
    }

    private void AddSession(int daysAgo, int showerMinutes = 10, bool complete = true)
    {
        var start = Now.AddDays(-daysAgo).AddHours(-12);
        var session = RoutineSession.Start(_routine, start).Value;
        session.CompleteCurrentStep(start.AddMinutes(1));
        if (complete)
            session.CompleteCurrentStep(start.AddMinutes(1 + showerMinutes));
        else
            session.EndEarly(start.AddMinutes(2));

        _sessions.Add(session);
    }

    private Task<ProgressSummary> Execute() =>
        new GetProgressSummary(_sessions, _time).ExecuteAsync(TestContext.Current.CancellationToken);

    [Fact]
    public async Task Empty_history_has_no_activity()
    {
        var summary = await Execute();

        summary.HasAnyActivity.ShouldBeFalse();
        summary.Days.Count.ShouldBe(GetProgressSummary.HeatmapDays);
        summary.Consistency.ActiveDays.ShouldBe(0);
    }

    [Fact]
    public async Task Summarizes_the_last_week_and_counts_partial_sessions()
    {
        AddSession(daysAgo: 0);
        AddSession(daysAgo: 1, complete: false);
        AddSession(daysAgo: 3);
        AddSession(daysAgo: 10);

        var summary = await Execute();

        summary.Consistency.ActiveDays.ShouldBe(3);
        summary.RoutinesCompleted.ShouldBe(2);
        summary.StepsDone.ShouldBe(5);
        summary.Days[^1].StepsDone.ShouldBe(2);
        summary.Days[^2].StepsDone.ShouldBe(1);
        summary.Days.Count(d => d.StepsDone > 0).ShouldBe(4);
    }

    [Fact]
    public async Task Suggests_longer_durations_for_steps_that_always_take_longer()
    {
        AddSession(daysAgo: 0, showerMinutes: 18);
        AddSession(daysAgo: 1, showerMinutes: 17);
        AddSession(daysAgo: 2, showerMinutes: 19);

        var summary = await Execute();

        var insight = summary.Insights.ShouldHaveSingleItem();
        insight.StepTitle.ShouldBe("Ducha");
        insight.TypicalActual.ShouldBe(TimeSpan.FromMinutes(18));
    }

    [Fact]
    public void Day_levels_scale_without_judgement()
    {
        new DayActivity(default, 0).Level.ShouldBe(0);
        new DayActivity(default, 2).Level.ShouldBe(1);
        new DayActivity(default, 6).Level.ShouldBe(2);
        new DayActivity(default, 12).Level.ShouldBe(3);
    }
}
