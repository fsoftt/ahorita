namespace Ahorita.Domain.Progress;

/// <summary>
/// "Ducharte te toma ~18 min, no 10": combate la ceguera temporal con datos reales.
/// Solo se sugiere con evidencia suficiente y una diferencia que importe.
/// </summary>
public sealed record DurationInsight(Guid RoutineId, Guid StepId, string StepTitle, string? StepEmoji, TimeSpan Planned, TimeSpan TypicalActual)
{
    public const int MinimumSamples = 3;
    public static readonly TimeSpan MinimumDifference = TimeSpan.FromMinutes(2);
    public const double MinimumRatio = 1.3;

    /// <summary>Duración sugerida, redondeada al minuto hacia arriba.</summary>
    public TimeSpan SuggestedDuration => TimeSpan.FromMinutes(Math.Ceiling(TypicalActual.TotalMinutes));

    public static DurationInsight? Evaluate(Guid routineId, Guid stepId, string title, string? emoji, TimeSpan planned, IReadOnlyCollection<TimeSpan> actuals)
    {
        if (actuals.Count < MinimumSamples || planned <= TimeSpan.Zero)
            return null;

        // Mediana: un día raro (te llamaron a mitad de la ducha) no distorsiona la sugerencia.
        var sorted = actuals.Order().ToArray();
        var median = sorted.Length % 2 == 1
            ? sorted[sorted.Length / 2]
            : (sorted[(sorted.Length / 2) - 1] + sorted[sorted.Length / 2]) / 2;

        var longEnough = median - planned >= MinimumDifference && median.TotalSeconds >= planned.TotalSeconds * MinimumRatio;
        return longEnough ? new DurationInsight(routineId, stepId, title, emoji, planned, median) : null;
    }
}
