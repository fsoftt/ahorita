using Ahorita.Application.Abstractions;

namespace Ahorita.Application.Routines;

/// <summary>Biblioteca de rutinas: primero las programadas por hora, luego las de "a demanda".</summary>
public sealed class ListRoutines(IRoutineRepository routines)
{
    public async Task<IReadOnlyList<RoutineSummary>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var active = await routines.ListActiveAsync(cancellationToken).ConfigureAwait(false);

        return active
            .Select(RoutineSummary.From)
            .OrderBy(r => r.IsOnDemand)
            .ThenBy(r => r.StartTime)
            .ThenBy(r => r.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }
}
