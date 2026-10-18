using Ahorita.Domain.Progress;

namespace Ahorita.Domain.Tests.Progress;

public sealed class ProgressTests
{
    private static readonly DateOnly Today = new(2026, 10, 19);

    [Fact]
    public void Consistency_counts_distinct_active_days_in_the_window()
    {
        DateOnly[] days = [Today, Today, Today.AddDays(-1), Today.AddDays(-6), Today.AddDays(-7), Today.AddDays(1)];

        var score = ConsistencyScore.From(days, Today);

        score.ActiveDays.ShouldBe(3);
        score.WindowDays.ShouldBe(7);
        score.Ratio.ShouldBe(3d / 7, tolerance: 0.001);
    }

    [Fact]
    public void A_missed_day_never_resets_consistency()
    {
        DateOnly[] days = [Today.AddDays(-6), Today.AddDays(-5), Today.AddDays(-4), Today.AddDays(-2), Today];

        ConsistencyScore.From(days, Today).ActiveDays.ShouldBe(5);
    }

    [Fact]
    public void Insight_requires_enough_samples()
    {
        var insight = DurationInsight.Evaluate(Guid.NewGuid(), Guid.NewGuid(), "Ducha", "🚿", TimeSpan.FromMinutes(10), [TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(20)]);

        insight.ShouldBeNull();
    }

    [Fact]
    public void Insight_uses_the_median_and_suggests_when_it_matters()
    {
        TimeSpan[] actuals = [TimeSpan.FromMinutes(17), TimeSpan.FromMinutes(18), TimeSpan.FromMinutes(60), TimeSpan.FromMinutes(19)];

        var insight = DurationInsight.Evaluate(Guid.NewGuid(), Guid.NewGuid(), "Ducha", "🚿", TimeSpan.FromMinutes(10), actuals);

        insight.ShouldNotBeNull();
        insight.TypicalActual.ShouldBe(TimeSpan.FromMinutes(18.5));
        insight.SuggestedDuration.ShouldBe(TimeSpan.FromMinutes(19));
    }

    [Theory]
    [InlineData(11)] // Menos de 2 min de diferencia.
    [InlineData(3)] // Más rápido de lo planeado: no se sugiere nada.
    public void Insight_ignores_small_or_negative_differences(int typicalMinutes)
    {
        TimeSpan[] actuals = [TimeSpan.FromMinutes(typicalMinutes), TimeSpan.FromMinutes(typicalMinutes), TimeSpan.FromMinutes(typicalMinutes)];

        DurationInsight.Evaluate(Guid.NewGuid(), Guid.NewGuid(), "Ducha", null, TimeSpan.FromMinutes(10), actuals).ShouldBeNull();
    }
}
