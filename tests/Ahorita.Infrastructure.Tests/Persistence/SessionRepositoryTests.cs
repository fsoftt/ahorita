using Ahorita.Domain.Routines;
using Ahorita.Domain.Sessions;
using Ahorita.Infrastructure.Persistence.Repositories;

namespace Ahorita.Infrastructure.Tests.Persistence;

public sealed class SessionRepositoryTests : SqliteTestDatabase
{
    private static readonly DateTimeOffset T0 = new(2026, 10, 19, 7, 0, 0, TimeSpan.FromHours(-6));

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static RoutineSession NewSession(DateTimeOffset startedAt)
    {
        var routine = Routine.Create("Mañana", "🌅", RoutineAccent.Tangerine, RoutineSchedule.OnDemand, T0).Value;
        routine.AddStep("Agua", "💧", TimeSpan.FromMinutes(1));
        routine.AddStep("Ducha", "🚿", TimeSpan.FromMinutes(10), isOptional: true);
        routine.AddStep("Vestirte");
        return RoutineSession.Start(routine, startedAt).Value;
    }

    [Fact]
    public async Task Round_trips_progress_postponed_steps_and_pauses()
    {
        var session = NewSession(T0);
        session.PostponeCurrentStep(T0.AddSeconds(5));
        session.CompleteCurrentStep(T0.AddMinutes(8));
        session.Pause(T0.AddMinutes(9));

        await using (var db = NewContext())
        {
            new SessionRepository(db).Add(session);
            await db.SaveChangesAsync(Ct);
        }

        await using (var db = NewContext())
        {
            var loaded = (await new SessionRepository(db).GetByIdAsync(session.Id, Ct))!;

            loaded.Status.ShouldBe(SessionStatus.Paused);
            loaded.StartedAt.ShouldBe(T0);
            loaded.PausedAt.ShouldBe(T0.AddMinutes(9));
            loaded.Steps.Select(s => s.Title).ShouldBe(["Ducha", "Vestirte", "Agua"]);
            loaded.Steps[0].Outcome.ShouldBe(StepOutcome.Done);
            loaded.Steps[0].IsOptional.ShouldBeTrue();
            loaded.Steps[2].WasPostponed.ShouldBeTrue();
            loaded.CurrentStep!.Title.ShouldBe("Vestirte");
            loaded.CurrentStepElapsed(T0.AddMinutes(30)).ShouldBe(TimeSpan.FromMinutes(1));
        }
    }

    [Fact]
    public async Task GetActive_returns_the_running_session_only()
    {
        var finished = NewSession(T0);
        finished.EndEarly(T0.AddMinutes(1));
        var running = NewSession(T0.AddMinutes(2));

        await using (var db = NewContext())
        {
            db.Sessions.AddRange(finished, running);
            await db.SaveChangesAsync(Ct);
        }

        await using (var db = NewContext())
        {
            (await new SessionRepository(db).GetActiveAsync(Ct))!.Id.ShouldBe(running.Id);
        }
    }

    [Fact]
    public async Task ListStartedBetween_filters_by_date_in_the_database()
    {
        var monday = NewSession(T0);
        var tuesday = NewSession(T0.AddDays(1));
        var nextWeek = NewSession(T0.AddDays(7));

        await using (var db = NewContext())
        {
            db.Sessions.AddRange(nextWeek, monday, tuesday);
            await db.SaveChangesAsync(Ct);
        }

        await using (var db = NewContext())
        {
            var list = await new SessionRepository(db).ListStartedBetweenAsync(T0, T0.AddDays(2), Ct);
            list.Select(s => s.Id).ShouldBe([monday.Id, tuesday.Id]);
        }
    }
}
