using Ahorita.Domain.Common;

namespace Ahorita.Domain.Routines;

/// <summary>
/// Rutina: una secuencia ordenada de pasos con identidad visual y horario.
/// Es la raíz del agregado; los pasos solo cambian a través de ella.
/// </summary>
public sealed class Routine : Entity<Guid>
{
    public const int MaxNameLength = 40;
    public const int MaxSteps = 30;

    /// <summary>A partir de aquí la UI sugiere (sin bloquear) una versión más corta.</summary>
    public const int RecommendedMaxSteps = 15;

    private readonly List<RoutineStep> _steps = [];

    private Routine(Guid id, string name, string emoji, RoutineAccent accent, RoutineSchedule schedule, DateTimeOffset createdAt)
        : base(id)
    {
        Name = name;
        Emoji = emoji;
        Accent = accent;
        Schedule = schedule;
        CreatedAt = createdAt;
    }

    // Requerido por EF Core para materializar el tipo propio Schedule.
    private Routine()
        : base(Guid.Empty)
    {
        Name = string.Empty;
        Emoji = string.Empty;
        Schedule = RoutineSchedule.OnDemand;
    }

    public string Name { get; private set; }
    public string Emoji { get; private set; }
    public RoutineAccent Accent { get; private set; }
    public RoutineSchedule Schedule { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyList<RoutineStep> Steps => _steps.OrderBy(s => s.Position).ToArray();

    /// <summary>Suma de las duraciones estimadas de los pasos que tienen duración.</summary>
    public TimeSpan EstimatedDuration => _steps.Aggregate(TimeSpan.Zero, (total, step) => total + (step.Duration ?? TimeSpan.Zero));

    /// <summary>Una rutina se puede guardar como borrador vacío, pero no iniciar.</summary>
    public bool CanStart => !IsArchived && _steps.Count > 0;

    public bool IsLong => _steps.Count > RecommendedMaxSteps;

    public static Result<Routine> Create(string name, string emoji, RoutineAccent accent, RoutineSchedule schedule, DateTimeOffset createdAt)
    {
        var validation = ValidateIdentity(name, emoji);
        if (validation.IsFailure)
            return validation.Error;

        return new Routine(Guid.CreateVersion7(), name.Trim(), emoji.Trim(), accent, schedule, createdAt);
    }

    public Result Rename(string name)
    {
        var validation = ValidateIdentity(name, Emoji);
        if (validation.IsFailure)
            return validation;

        Name = name.Trim();
        return Result.Success();
    }

    public Result ChangeAppearance(string emoji, RoutineAccent accent)
    {
        var validation = ValidateIdentity(Name, emoji);
        if (validation.IsFailure)
            return validation;

        Emoji = emoji.Trim();
        Accent = accent;
        return Result.Success();
    }

    public void Reschedule(RoutineSchedule schedule) => Schedule = schedule;

    public void Archive() => IsArchived = true;

    public void Restore() => IsArchived = false;

    public Result<RoutineStep> AddStep(string title, string? emoji = null, TimeSpan? duration = null, bool isOptional = false)
    {
        if (_steps.Count >= MaxSteps)
            return RoutineErrors.TooManySteps;

        var step = RoutineStep.Create(title, emoji, duration, isOptional, position: _steps.Count);
        if (step.IsSuccess)
            _steps.Add(step.Value);

        return step;
    }

    public Result UpdateStep(Guid stepId, string title, string? emoji, TimeSpan? duration, bool isOptional) =>
        FindStep(stepId) is { } step
            ? step.Update(title, emoji, duration, isOptional)
            : RoutineErrors.StepNotFound;

    public Result RemoveStep(Guid stepId)
    {
        if (FindStep(stepId) is not { } step)
            return RoutineErrors.StepNotFound;

        _steps.Remove(step);
        Reindex(Steps);
        return Result.Success();
    }

    /// <summary>Mueve un paso a <paramref name="newIndex"/> (base 0) desplazando a los demás.</summary>
    public Result MoveStep(Guid stepId, int newIndex)
    {
        if (FindStep(stepId) is not { } step)
            return RoutineErrors.StepNotFound;

        if (newIndex < 0 || newIndex >= _steps.Count)
            return RoutineErrors.InvalidStepPosition;

        var ordered = Steps.ToList();
        ordered.Remove(step);
        ordered.Insert(newIndex, step);
        Reindex(ordered);
        return Result.Success();
    }

    private RoutineStep? FindStep(Guid stepId) => _steps.Find(s => s.Id == stepId);

    private static void Reindex(IReadOnlyList<RoutineStep> ordered)
    {
        for (var i = 0; i < ordered.Count; i++)
            ordered[i].Position = i;
    }

    private static Result ValidateIdentity(string name, string emoji)
    {
        if (string.IsNullOrWhiteSpace(name))
            return RoutineErrors.NameRequired;

        if (name.Trim().Length > MaxNameLength)
            return RoutineErrors.NameTooLong;

        if (string.IsNullOrWhiteSpace(emoji))
            return RoutineErrors.EmojiRequired;

        return Result.Success();
    }
}
