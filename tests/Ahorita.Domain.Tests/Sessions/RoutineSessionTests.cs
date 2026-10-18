using Ahorita.Domain.Routines;
using Ahorita.Domain.Sessions;

namespace Ahorita.Domain.Tests.Sessions;

public sealed class RoutineSessionTests
{
    private static readonly DateTimeOffset T0 = new(2026, 10, 19, 7, 0, 0, TimeSpan.Zero);

    private static Routine Morning(bool optionalLast = false)
    {
        var routine = Routine.Create("Mañana", "🌅", RoutineAccent.Tangerine, RoutineSchedule.OnDemand, T0).Value;
        routine.AddStep("Agua", "💧", TimeSpan.FromMinutes(1));
        routine.AddStep("Ducha", "🚿", TimeSpan.FromMinutes(10));
        routine.AddStep("Leer", "📖", TimeSpan.FromMinutes(5), isOptional: optionalLast);
        return routine;
    }

    private static RoutineSession Start(Routine? routine = null) => RoutineSession.Start(routine ?? Morning(), T0).Value;

    [Fact]
    public void Start_snapshots_the_routine_and_begins_with_the_first_step()
    {
        var session = Start();

        session.Status.ShouldBe(SessionStatus.Running);
        session.RoutineName.ShouldBe("Mañana");
        session.TotalSteps.ShouldBe(3);
        session.CurrentStep!.Title.ShouldBe("Agua");
        session.NextStep!.Title.ShouldBe("Ducha");
        session.CurrentStepStartedAt.ShouldBe(T0);
    }

    [Fact]
    public void Cannot_start_an_empty_routine()
    {
        var empty = Routine.Create("Vacía", "🫥", RoutineAccent.Mint, RoutineSchedule.OnDemand, T0).Value;

        RoutineSession.Start(empty, T0).Error.ShouldBe(SessionErrors.RoutineCannotStart);
    }

    [Fact]
    public void Editing_the_routine_does_not_change_a_running_session()
    {
        var routine = Morning();
        var session = Start(routine);

        routine.Rename("Otra");
        routine.AddStep("Extra");

        session.RoutineName.ShouldBe("Mañana");
        session.TotalSteps.ShouldBe(3);
    }

    [Fact]
    public void Completing_steps_records_actual_duration_and_advances()
    {
        var session = Start();

        session.CompleteCurrentStep(T0.AddSeconds(90)).IsSuccess.ShouldBeTrue();

        session.Steps[0].Outcome.ShouldBe(StepOutcome.Done);
        session.Steps[0].ActualDuration.ShouldBe(TimeSpan.FromSeconds(90));
        session.CurrentStep!.Title.ShouldBe("Ducha");
        session.CurrentStepStartedAt.ShouldBe(T0.AddSeconds(90));
    }

    [Fact]
    public void Resolving_every_step_completes_the_session()
    {
        var session = Start();

        session.CompleteCurrentStep(T0.AddMinutes(1));
        session.SkipCurrentStep(T0.AddMinutes(2));
        session.CompleteCurrentStep(T0.AddMinutes(3));

        session.Status.ShouldBe(SessionStatus.Completed);
        session.FinishedAt.ShouldBe(T0.AddMinutes(3));
        session.CurrentStep.ShouldBeNull();
        session.CompleteCurrentStep(T0.AddMinutes(4)).Error.ShouldBe(SessionErrors.NotActive);
    }

    [Fact]
    public void CompletionRatio_counts_partial_progress()
    {
        var session = Start();
        session.CompleteCurrentStep(T0.AddMinutes(1));
        session.SkipCurrentStep(T0.AddMinutes(2));
        session.CompleteCurrentStep(T0.AddMinutes(3));

        session.CompletionRatio.ShouldBe(2d / 3, tolerance: 0.001);
    }

    [Fact]
    public void Skipped_optional_steps_do_not_count_against_progress()
    {
        var session = Start(Morning(optionalLast: true));
        session.CompleteCurrentStep(T0.AddMinutes(1));
        session.CompleteCurrentStep(T0.AddMinutes(2));
        session.SkipCurrentStep(T0.AddMinutes(3));

        session.CompletionRatio.ShouldBe(1);
    }

    [Fact]
    public void Postpone_moves_the_step_to_the_end_only_once()
    {
        var session = Start();

        session.PostponeCurrentStep(T0.AddSeconds(10)).IsSuccess.ShouldBeTrue();

        session.CurrentStep!.Title.ShouldBe("Ducha");
        session.Steps.Select(s => s.Title).ShouldBe(["Ducha", "Leer", "Agua"]);

        session.CompleteCurrentStep(T0.AddMinutes(1));
        session.CompleteCurrentStep(T0.AddMinutes(2));
        session.CurrentStep!.Title.ShouldBe("Agua");
        session.CanPostponeCurrent.ShouldBeFalse();
        session.PostponeCurrentStep(T0.AddMinutes(3)).Error.ShouldBe(SessionErrors.CannotPostpone);
    }

    [Fact]
    public void Cannot_postpone_the_last_pending_step()
    {
        var session = Start();
        session.CompleteCurrentStep(T0.AddMinutes(1));
        session.CompleteCurrentStep(T0.AddMinutes(2));

        session.PostponeCurrentStep(T0.AddMinutes(3)).Error.ShouldBe(SessionErrors.CannotPostpone);
    }

    [Fact]
    public void Paused_time_is_not_counted()
    {
        var session = Start();

        session.Pause(T0.AddSeconds(20)).IsSuccess.ShouldBeTrue();
        session.CurrentStepElapsed(T0.AddMinutes(5)).ShouldBe(TimeSpan.FromSeconds(20));

        session.Resume(T0.AddSeconds(80)).IsSuccess.ShouldBeTrue();
        session.CurrentStepElapsed(T0.AddSeconds(90)).ShouldBe(TimeSpan.FromSeconds(30));

        session.CompleteCurrentStep(T0.AddSeconds(100));
        session.Steps[0].ActualDuration.ShouldBe(TimeSpan.FromSeconds(40));
    }

    [Fact]
    public void Completing_while_paused_ends_the_pause()
    {
        var session = Start();
        session.Pause(T0.AddSeconds(10));

        session.CompleteCurrentStep(T0.AddSeconds(60));

        session.Status.ShouldBe(SessionStatus.Running);
        session.Steps[0].ActualDuration.ShouldBe(TimeSpan.FromSeconds(10));
        session.CurrentStepElapsed(T0.AddSeconds(70)).ShouldBe(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Pause_and_resume_validate_state()
    {
        var session = Start();

        session.Resume(T0).Error.ShouldBe(SessionErrors.NotPaused);
        session.Pause(T0);
        session.Pause(T0).Error.ShouldBe(SessionErrors.AlreadyPaused);
    }

    [Fact]
    public void Remaining_time_goes_negative_when_running_over()
    {
        var session = Start();

        session.CurrentStepRemaining(T0.AddSeconds(30)).ShouldBe(TimeSpan.FromSeconds(30));
        session.CurrentStepRemaining(T0.AddSeconds(75)).ShouldBe(TimeSpan.FromSeconds(-15));
    }

    [Fact]
    public void EstimatedEnd_adds_remaining_and_upcoming_planned_time()
    {
        var session = Start();

        // 1 min de agua + 10 de ducha + 5 de lectura.
        session.EstimatedEnd(T0).ShouldBe(T0.AddMinutes(16));

        // Si el paso actual se pasó de tiempo, no resta: el resto empieza "ya".
        session.EstimatedEnd(T0.AddMinutes(3)).ShouldBe(T0.AddMinutes(3 + 15));
    }

    [Fact]
    public void EndEarly_keeps_what_was_done()
    {
        var session = Start();
        session.CompleteCurrentStep(T0.AddMinutes(1));

        session.EndEarly(T0.AddMinutes(2)).IsSuccess.ShouldBeTrue();

        session.Status.ShouldBe(SessionStatus.EndedEarly);
        session.IsActive.ShouldBeFalse();
        session.DoneSteps.ShouldBe(1);
        session.FinishedAt.ShouldBe(T0.AddMinutes(2));
        session.EndEarly(T0.AddMinutes(3)).Error.ShouldBe(SessionErrors.NotActive);
    }
}
