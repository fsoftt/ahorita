using Ahorita.Domain.Routines;
using Ahorita.Domain.Sessions;

namespace Ahorita.Application.Sessions;

/// <summary>
/// Foto de una sesión en <see cref="TakenAt"/>. La UI la "anima" sumando el tiempo
/// transcurrido desde entonces, sin volver a consultar la base de datos cada segundo.
/// </summary>
public sealed record SessionSnapshot(
    Guid Id,
    Guid RoutineId,
    string RoutineName,
    string RoutineEmoji,
    RoutineAccent Accent,
    SessionStatus Status,
    SessionStepView? CurrentStep,
    SessionStepView? NextStep,
    int Position,
    int TotalSteps,
    int DoneSteps,
    double CompletionRatio,
    bool CanPostpone,
    TimeSpan CurrentStepElapsed,
    TimeSpan PlannedAfterCurrent,
    TimeSpan TotalElapsed,
    DateTimeOffset TakenAt)
{
    public bool IsActive => Status is SessionStatus.Running or SessionStatus.Paused;
    public bool IsPaused => Status == SessionStatus.Paused;
    public bool IsFinished => !IsActive;

    /// <summary>Tiempo efectivo en el paso actual en el instante <paramref name="now"/>.</summary>
    public TimeSpan ElapsedAt(DateTimeOffset now) =>
        Status == SessionStatus.Running ? CurrentStepElapsed + (now - TakenAt) : CurrentStepElapsed;

    public TimeSpan? RemainingAt(DateTimeOffset now) =>
        CurrentStep?.PlannedDuration is { } planned ? planned - ElapsedAt(now) : null;

    public DateTimeOffset EstimatedEndAt(DateTimeOffset now)
    {
        var remaining = RemainingAt(now) ?? TimeSpan.Zero;
        return now + (remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero) + PlannedAfterCurrent;
    }

    /// <summary>Cuándo termina el tiempo planeado del paso actual (solo si corre y tiene duración).</summary>
    public DateTimeOffset? CurrentStepDeadline =>
        Status == SessionStatus.Running && CurrentStep?.PlannedDuration is { } planned
            ? TakenAt + (planned - CurrentStepElapsed)
            : null;

    public static SessionSnapshot From(RoutineSession session, DateTimeOffset now) => new(
        session.Id,
        session.RoutineId,
        session.RoutineName,
        session.RoutineEmoji,
        session.RoutineAccent,
        session.Status,
        session.CurrentStep is { } current ? SessionStepView.From(current) : null,
        session.NextStep is { } next ? SessionStepView.From(next) : null,
        Math.Min(session.ResolvedSteps + 1, session.TotalSteps),
        session.TotalSteps,
        session.DoneSteps,
        session.CompletionRatio,
        session.CanPostponeCurrent,
        session.CurrentStepElapsed(now),
        session.PlannedAfterCurrent,
        (session.FinishedAt ?? now) - session.StartedAt,
        now);
}

public sealed record SessionStepView(string Title, string? Emoji, TimeSpan? PlannedDuration, bool IsOptional)
{
    public static SessionStepView From(SessionStep step) => new(step.Title, step.Emoji, step.PlannedDuration, step.IsOptional);
}
