using Ahorita.Application.Abstractions;
using Ahorita.Domain.Routines;

namespace Ahorita.Application.Routines;

/// <summary>
/// Qué mostrar en la pantalla "Hoy". Las rutinas cuyo horario ya pasó no aparecen:
/// nada de listas de pendientes vencidos que generen culpa.
/// </summary>
public sealed class GetTodayOverview(IRoutineRepository routines, TimeProvider time)
{
    /// <summary>Margen mínimo durante el cual una rutina ya iniciada sigue siendo "la de ahora".</summary>
    public static readonly TimeSpan MinimumActiveWindow = TimeSpan.FromMinutes(30);

    public async Task<TodayOverview> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var active = await routines.ListActiveAsync(cancellationToken).ConfigureAwait(false);

        var now = time.GetLocalNow();
        var nowOfDay = now.TimeOfDay;
        var startable = active.Where(r => r.CanStart).ToArray();

        var upcoming = startable
            .Where(r => r.Schedule.IsScheduledOn(now.DayOfWeek) && IsStillRelevant(r, nowOfDay))
            .OrderBy(r => r.Schedule.StartTime)
            .Select(RoutineSummary.From)
            .ToList();

        var anyTime = startable
            .Where(r => r.Schedule.IsOnDemand)
            .OrderBy(r => r.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(RoutineSummary.From)
            .ToList();

        var featured = upcoming.FirstOrDefault() ?? anyTime.FirstOrDefault();
        if (featured is not null)
        {
            upcoming.Remove(featured);
            anyTime.Remove(featured);
        }

        return new TodayOverview(featured, upcoming, anyTime, active.Count);
    }

    private static bool IsStillRelevant(Routine routine, TimeSpan nowOfDay)
    {
        var start = routine.Schedule.StartTime!.Value.ToTimeSpan();
        var window = routine.EstimatedDuration > MinimumActiveWindow ? routine.EstimatedDuration : MinimumActiveWindow;
        return start + window > nowOfDay;
    }
}

/// <param name="Featured">La rutina protagonista: la próxima programada o, si no hay, una "a demanda".</param>
/// <param name="Upcoming">Resto de rutinas programadas para más tarde hoy.</param>
/// <param name="AnyTime">Rutinas "a demanda" que no son la protagonista.</param>
/// <param name="TotalRoutines">Total de rutinas activas, para distinguir "día libre" de "primer uso".</param>
public sealed record TodayOverview(
    RoutineSummary? Featured,
    IReadOnlyList<RoutineSummary> Upcoming,
    IReadOnlyList<RoutineSummary> AnyTime,
    int TotalRoutines)
{
    public bool IsFirstRun => TotalRoutines == 0;
}
