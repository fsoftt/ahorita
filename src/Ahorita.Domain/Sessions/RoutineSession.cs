using Ahorita.Domain.Common;
using Ahorita.Domain.Routines;

namespace Ahorita.Domain.Sessions;

/// <summary>
/// Una ejecución concreta de una rutina ("modo Ahora"). Muestra un paso a la vez;
/// el tiempo se calcula con marcas de tiempo, así sobrevive a que la app se cierre.
/// </summary>
public sealed class RoutineSession : Entity<Guid>
{
    private readonly List<SessionStep> _steps = [];

    private RoutineSession(Guid id, Routine routine, DateTimeOffset now)
        : base(id)
    {
        RoutineId = routine.Id;
        RoutineName = routine.Name;
        RoutineEmoji = routine.Emoji;
        RoutineAccent = routine.Accent;
        StartedAt = now;
        CurrentStepStartedAt = now;
    }

    // Requerido por EF Core.
    private RoutineSession()
        : base(Guid.Empty)
    {
        RoutineName = string.Empty;
        RoutineEmoji = string.Empty;
    }

    public Guid RoutineId { get; private set; }
    public string RoutineName { get; private set; }
    public string RoutineEmoji { get; private set; }
    public RoutineAccent RoutineAccent { get; private set; }
    public SessionStatus Status { get; private set; } = SessionStatus.Running;
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }
    public DateTimeOffset CurrentStepStartedAt { get; private set; }
    public DateTimeOffset? PausedAt { get; private set; }

    /// <summary>Tiempo en pausa acumulado durante el paso actual.</summary>
    public TimeSpan CurrentStepPausedTotal { get; private set; }

    public IReadOnlyList<SessionStep> Steps => _steps.OrderBy(s => s.Order).ToArray();

    public bool IsActive => Status is SessionStatus.Running or SessionStatus.Paused;
    public bool IsPaused => Status == SessionStatus.Paused;

    public SessionStep? CurrentStep => IsActive ? Steps.FirstOrDefault(s => s.IsPending) : null;
    public SessionStep? NextStep => IsActive ? Steps.Where(s => s.IsPending).Skip(1).FirstOrDefault() : null;

    public int TotalSteps => _steps.Count;
    public int ResolvedSteps => _steps.Count(s => !s.IsPending);
    public int DoneSteps => _steps.Count(s => s.Outcome == StepOutcome.Done);

    /// <summary>
    /// Proporción completada. Los pasos opcionales saltados no cuentan en contra:
    /// 3 de 5 obligatorios es 60 %, y eso también se celebra.
    /// </summary>
    public double CompletionRatio
    {
        get
        {
            var counted = _steps.Count(s => !(s.IsOptional && s.Outcome == StepOutcome.Skipped));
            return counted == 0 ? 1 : (double)DoneSteps / counted;
        }
    }

    public bool CanPostponeCurrent =>
        CurrentStep is { WasPostponed: false } && _steps.Count(s => s.IsPending) > 1;

    public static Result<RoutineSession> Start(Routine routine, DateTimeOffset now)
    {
        if (!routine.CanStart)
            return SessionErrors.RoutineCannotStart;

        var session = new RoutineSession(Guid.CreateVersion7(), routine, now);
        var steps = routine.Steps;
        for (var i = 0; i < steps.Count; i++)
            session._steps.Add(SessionStep.CopyOf(steps[i], i));

        return session;
    }

    /// <summary>Tiempo efectivo (sin pausas) en el paso actual.</summary>
    public TimeSpan CurrentStepElapsed(DateTimeOffset now)
    {
        var reference = PausedAt ?? now;
        var elapsed = reference - CurrentStepStartedAt - CurrentStepPausedTotal;
        return elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
    }

    /// <summary>Tiempo restante del paso actual; negativo si se pasó (sin castigo, solo informa).</summary>
    public TimeSpan? CurrentStepRemaining(DateTimeOffset now) =>
        CurrentStep?.PlannedDuration is { } planned ? planned - CurrentStepElapsed(now) : null;

    /// <summary>Suma de duraciones planeadas de los pasos pendientes después del actual.</summary>
    public TimeSpan PlannedAfterCurrent =>
        Steps.Where(s => s.IsPending).Skip(1).Aggregate(TimeSpan.Zero, (total, s) => total + (s.PlannedDuration ?? TimeSpan.Zero));

    /// <summary>"Terminas a las 8:12": combate la ceguera temporal.</summary>
    public DateTimeOffset? EstimatedEnd(DateTimeOffset now)
    {
        if (!IsActive)
            return FinishedAt;

        var remaining = CurrentStepRemaining(now) ?? TimeSpan.Zero;
        return now + (remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero) + PlannedAfterCurrent;
    }

    public Result CompleteCurrentStep(DateTimeOffset now) => ResolveCurrent(StepOutcome.Done, now);

    public Result SkipCurrentStep(DateTimeOffset now) => ResolveCurrent(StepOutcome.Skipped, now);

    /// <summary>"Lo hago después": manda el paso al final de la cola, una sola vez.</summary>
    public Result PostponeCurrentStep(DateTimeOffset now)
    {
        if (!IsActive)
            return SessionErrors.NotActive;

        if (!CanPostponeCurrent)
            return SessionErrors.CannotPostpone;

        CurrentStep!.MarkPostponed(_steps.Max(s => s.Order) + 1);
        StartNextStep(now);
        return Result.Success();
    }

    public Result Pause(DateTimeOffset now)
    {
        if (!IsActive)
            return SessionErrors.NotActive;

        if (IsPaused)
            return SessionErrors.AlreadyPaused;

        Status = SessionStatus.Paused;
        PausedAt = now;
        return Result.Success();
    }

    public Result Resume(DateTimeOffset now)
    {
        if (!IsPaused)
            return SessionErrors.NotPaused;

        EndPause(now);
        return Result.Success();
    }

    /// <summary>Terminar antes es válido con cualquier progreso; nunca se presenta como fallo.</summary>
    public Result EndEarly(DateTimeOffset now)
    {
        if (!IsActive)
            return SessionErrors.NotActive;

        EndPause(now);
        Status = SessionStatus.EndedEarly;
        FinishedAt = now;
        return Result.Success();
    }

    private Result ResolveCurrent(StepOutcome outcome, DateTimeOffset now)
    {
        if (CurrentStep is not { } step)
            return SessionErrors.NotActive;

        EndPause(now);
        step.Resolve(outcome, CurrentStepElapsed(now));

        if (_steps.TrueForAll(s => !s.IsPending))
        {
            Status = SessionStatus.Completed;
            FinishedAt = now;
        }
        else
        {
            StartNextStep(now);
        }

        return Result.Success();
    }

    private void StartNextStep(DateTimeOffset now)
    {
        CurrentStepStartedAt = now;
        CurrentStepPausedTotal = TimeSpan.Zero;
    }

    private void EndPause(DateTimeOffset now)
    {
        if (PausedAt is { } pausedAt)
            CurrentStepPausedTotal += now - pausedAt;

        PausedAt = null;
        if (Status == SessionStatus.Paused)
            Status = SessionStatus.Running;
    }
}
