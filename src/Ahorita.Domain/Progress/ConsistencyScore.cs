namespace Ahorita.Domain.Progress;

/// <summary>
/// Constancia flexible: "5 de los últimos 7 días". No existe la "racha rota":
/// un día sin rutina solo baja un poco el número, nunca lo reinicia.
/// </summary>
public sealed record ConsistencyScore(int ActiveDays, int WindowDays)
{
    public double Ratio => WindowDays == 0 ? 0 : (double)ActiveDays / WindowDays;

    public static ConsistencyScore From(IEnumerable<DateOnly> activeDays, DateOnly today, int windowDays = 7)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(windowDays);

        var from = today.AddDays(-(windowDays - 1));
        var count = activeDays.Where(d => d >= from && d <= today).Distinct().Count();
        return new ConsistencyScore(count, windowDays);
    }
}
