using Ahorita.Domain.Common;

namespace Ahorita.Domain.Sessions;

/// <summary>
/// Copia de un paso al iniciar la sesión: si la rutina se edita a mitad,
/// la sesión en curso no cambia.
/// </summary>
public sealed class SessionStep : Entity<Guid>
{
    private SessionStep(Guid id, Guid sourceStepId, string title, string? emoji, TimeSpan? plannedDuration, bool isOptional, int order)
        : base(id)
    {
        SourceStepId = sourceStepId;
        Title = title;
        Emoji = emoji;
        PlannedDuration = plannedDuration;
        IsOptional = isOptional;
        Order = order;
    }

    public Guid SourceStepId { get; private set; }
    public string Title { get; private set; }
    public string? Emoji { get; private set; }
    public TimeSpan? PlannedDuration { get; private set; }
    public bool IsOptional { get; private set; }

    /// <summary>Posición en la cola; cambia al posponer.</summary>
    public int Order { get; internal set; }

    public StepOutcome Outcome { get; private set; } = StepOutcome.Pending;
    public bool WasPostponed { get; private set; }
    public TimeSpan? ActualDuration { get; private set; }

    public bool IsPending => Outcome == StepOutcome.Pending;

    internal static SessionStep CopyOf(Routines.RoutineStep step, int order) =>
        new(Guid.CreateVersion7(), step.Id, step.Title, step.Emoji, step.Duration, step.IsOptional, order);

    internal void Resolve(StepOutcome outcome, TimeSpan actualDuration)
    {
        Outcome = outcome;
        ActualDuration = actualDuration < TimeSpan.Zero ? TimeSpan.Zero : actualDuration;
    }

    internal void MarkPostponed(int newOrder)
    {
        WasPostponed = true;
        Order = newOrder;
    }
}
