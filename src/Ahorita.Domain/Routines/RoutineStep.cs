using Ahorita.Domain.Common;

namespace Ahorita.Domain.Routines;

/// <summary>Un paso de una rutina. Solo se modifica a través de <see cref="Routine"/>.</summary>
public sealed class RoutineStep : Entity<Guid>
{
    public const int MaxTitleLength = 60;
    public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(4);

    private RoutineStep(Guid id, string title, string? emoji, TimeSpan? duration, bool isOptional, int position)
        : base(id)
    {
        Title = title;
        Emoji = emoji;
        Duration = duration;
        IsOptional = isOptional;
        Position = position;
    }

    public string Title { get; private set; }
    public string? Emoji { get; private set; }

    /// <summary>Duración estimada. Sin duración, el paso no usa temporizador.</summary>
    public TimeSpan? Duration { get; private set; }

    /// <summary>Paso de "si me da tiempo": se puede saltar sin afectar el progreso.</summary>
    public bool IsOptional { get; private set; }

    public int Position { get; internal set; }

    internal static Result<RoutineStep> Create(string title, string? emoji, TimeSpan? duration, bool isOptional, int position)
    {
        var validation = Validate(title, duration);
        if (validation.IsFailure)
            return validation.Error;

        return new RoutineStep(Guid.CreateVersion7(), title.Trim(), Normalize(emoji), duration, isOptional, position);
    }

    internal Result Update(string title, string? emoji, TimeSpan? duration, bool isOptional)
    {
        var validation = Validate(title, duration);
        if (validation.IsFailure)
            return validation;

        Title = title.Trim();
        Emoji = Normalize(emoji);
        Duration = duration;
        IsOptional = isOptional;
        return Result.Success();
    }

    private static Result Validate(string title, TimeSpan? duration)
    {
        if (string.IsNullOrWhiteSpace(title))
            return RoutineErrors.StepTitleRequired;

        if (title.Trim().Length > MaxTitleLength)
            return RoutineErrors.StepTitleTooLong;

        if (duration is { } d && (d <= TimeSpan.Zero || d > MaxDuration))
            return RoutineErrors.InvalidStepDuration;

        return Result.Success();
    }

    private static string? Normalize(string? emoji) => string.IsNullOrWhiteSpace(emoji) ? null : emoji.Trim();
}
