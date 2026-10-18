using Ahorita.Application.Sessions;
using Ahorita.Domain.Routines;
using Ahorita.Domain.Sessions;

namespace Ahorita.Application.Tests.Sessions;

public sealed class SessionSnapshotTests
{
    private static readonly DateTimeOffset T0 = new(2026, 10, 19, 7, 0, 0, TimeSpan.Zero);

    private static RoutineSession NewSession()
    {
        var routine = Routine.Create("Mañana", "🌅", RoutineAccent.Tangerine, RoutineSchedule.OnDemand, T0).Value;
        routine.AddStep("Agua", duration: TimeSpan.FromMinutes(2));
        routine.AddStep("Ducha", duration: TimeSpan.FromMinutes(10));
        return RoutineSession.Start(routine, T0).Value;
    }

    [Fact]
    public void Running_snapshot_projects_time_forward()
    {
        var snapshot = SessionSnapshot.From(NewSession(), T0.AddSeconds(30));

        snapshot.ElapsedAt(T0.AddSeconds(90)).ShouldBe(TimeSpan.FromSeconds(90));
        snapshot.RemainingAt(T0.AddSeconds(90)).ShouldBe(TimeSpan.FromSeconds(30));
        snapshot.CurrentStepDeadline.ShouldBe(T0.AddMinutes(2));
        snapshot.EstimatedEndAt(T0.AddSeconds(90)).ShouldBe(T0.AddMinutes(12));
    }

    [Fact]
    public void Paused_snapshot_freezes_time_and_has_no_deadline()
    {
        var session = NewSession();
        session.Pause(T0.AddSeconds(20));

        var snapshot = SessionSnapshot.From(session, T0.AddSeconds(25));

        snapshot.IsPaused.ShouldBeTrue();
        snapshot.ElapsedAt(T0.AddMinutes(10)).ShouldBe(TimeSpan.FromSeconds(20));
        snapshot.CurrentStepDeadline.ShouldBeNull();
    }

    [Fact]
    public void Exposes_current_and_next_steps()
    {
        var snapshot = SessionSnapshot.From(NewSession(), T0);

        snapshot.CurrentStep!.Title.ShouldBe("Agua");
        snapshot.NextStep!.Title.ShouldBe("Ducha");
        snapshot.CanPostpone.ShouldBeTrue();
        snapshot.TotalSteps.ShouldBe(2);
    }
}
