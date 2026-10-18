using Ahorita.Application.Abstractions;
using Ahorita.Domain.Common;

namespace Ahorita.Application.Routines;

public sealed class GetRoutineForEditing(IRoutineRepository routines)
{
    public async Task<Result<RoutineDraft>> ExecuteAsync(Guid routineId, CancellationToken cancellationToken = default)
    {
        var routine = await routines.GetByIdAsync(routineId, cancellationToken).ConfigureAwait(false);
        return routine is null ? ApplicationErrors.RoutineNotFound : RoutineDraft.From(routine);
    }
}
