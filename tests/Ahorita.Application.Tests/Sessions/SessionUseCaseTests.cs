using Ahorita.Application.Abstractions;
using Ahorita.Application.Sessions;
using Ahorita.Application.Tests.Fakes;
using Ahorita.Domain.Routines;
using Ahorita.Domain.Sessions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Ahorita.Application.Tests.Sessions;

public sealed class SessionUseCaseTests
{
    private static readonly DateTimeOffset T0 = new(2026, 10, 19, 7, 0, 0, TimeSpan.Zero);

    private readonly InMemoryRoutineRepository _routines = new();
    private readonly InMemorySessionRepository _sessions = new();
    private readonly IActiveRoutineNotifier _notifier = Substitute.For<IActiveRoutineNotifier>();
    private readonly FakeTimeProvider _time = new(T0);

    private StartSession Starter => new(_routines, _sessions, _routines, _notifier, _time);

    private ApplySessionAction Actions => new(_sessions, _routines, _notifier, _time);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private Routine AddRoutine(string name = "Mañana", int steps = 2)
    {
        var routine = Fakes.Routines.OnDemand(name, steps);
        _routines.Add(routine);
        return routine;
    }

    [Fact]
    public async Task Start_creates_a_session_and_shows_the_notification()
    {
        var routine = AddRoutine();

        var result = await Starter.ExecuteAsync(routine.Id, Ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.RoutineName.ShouldBe("Mañana");
        result.Value.CurrentStep!.Title.ShouldBe("Paso 1");
        result.Value.Position.ShouldBe(1);
        _sessions.All.ShouldHaveSingleItem();
        await _notifier.Received(1).ShowAsync(Arg.Any<SessionSnapshot>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Starting_the_same_routine_again_resumes_it()
    {
        var routine = AddRoutine();
        var first = await Starter.ExecuteAsync(routine.Id, Ct);

        var second = await Starter.ExecuteAsync(routine.Id, Ct);

        second.Value.Id.ShouldBe(first.Value.Id);
        _sessions.All.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Starting_another_routine_ends_the_previous_one_early()
    {
        var morning = AddRoutine("Mañana");
        var night = AddRoutine("Noche");
        await Starter.ExecuteAsync(morning.Id, Ct);

        await Starter.ExecuteAsync(night.Id, Ct);

        _sessions.All[0].Status.ShouldBe(SessionStatus.EndedEarly);
        _sessions.All[1].IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Start_fails_for_unknown_or_empty_routines()
    {
        var empty = Routine.Create("Vacía", "🫥", RoutineAccent.Mint, RoutineSchedule.OnDemand, T0).Value;
        _routines.Add(empty);

        (await Starter.ExecuteAsync(Guid.NewGuid(), Ct)).Error.ShouldBe(ApplicationErrors.RoutineNotFound);
        (await Starter.ExecuteAsync(empty.Id, Ct)).Error.ShouldBe(SessionErrors.RoutineCannotStart);
    }

    [Fact]
    public async Task Actions_advance_the_session_and_clear_the_notification_at_the_end()
    {
        var routine = AddRoutine(steps: 2);
        var session = (await Starter.ExecuteAsync(routine.Id, Ct)).Value;

        _time.Advance(TimeSpan.FromMinutes(1));
        var afterFirst = await Actions.ExecuteAsync(session.Id, SessionAction.CompleteStep, Ct);
        afterFirst.Value.Position.ShouldBe(2);

        _time.Advance(TimeSpan.FromMinutes(1));
        var afterSecond = await Actions.ExecuteAsync(session.Id, SessionAction.SkipStep, Ct);

        afterSecond.Value.IsFinished.ShouldBeTrue();
        afterSecond.Value.DoneSteps.ShouldBe(1);
        afterSecond.Value.TotalElapsed.ShouldBe(TimeSpan.FromMinutes(2));
        await _notifier.Received(1).ClearAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Domain_rule_failures_are_returned_without_saving()
    {
        var routine = AddRoutine(steps: 1);
        var session = (await Starter.ExecuteAsync(routine.Id, Ct)).Value;
        var savesBefore = _routines.SaveCount;

        var result = await Actions.ExecuteAsync(session.Id, SessionAction.PostponeStep, Ct);

        result.Error.ShouldBe(SessionErrors.CannotPostpone);
        _routines.SaveCount.ShouldBe(savesBefore);
    }

    [Fact]
    public async Task Unknown_sessions_fail_gracefully()
    {
        (await Actions.ExecuteAsync(Guid.NewGuid(), SessionAction.Pause, Ct)).Error.ShouldBe(SessionErrors.NotFound);
        (await new GetSession(_sessions, _time).ExecuteAsync(Guid.NewGuid(), Ct)).Error.ShouldBe(SessionErrors.NotFound);
    }

    [Fact]
    public async Task GetActiveSession_returns_the_running_one_only()
    {
        var getActive = new GetActiveSession(_sessions, _time);
        (await getActive.ExecuteAsync(Ct)).ShouldBeNull();

        var routine = AddRoutine();
        var session = (await Starter.ExecuteAsync(routine.Id, Ct)).Value;
        (await getActive.ExecuteAsync(Ct))!.Id.ShouldBe(session.Id);

        await Actions.ExecuteAsync(session.Id, SessionAction.EndEarly, Ct);
        (await getActive.ExecuteAsync(Ct)).ShouldBeNull();
    }
}
