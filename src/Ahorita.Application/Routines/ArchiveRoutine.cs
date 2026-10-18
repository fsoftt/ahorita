using Ahorita.Application.Abstractions;
using Ahorita.Domain.Common;

namespace Ahorita.Application.Routines;

/// <summary>"Eliminar" desde la UI: la rutina se archiva para no perder su historial.</summary>
public sealed class ArchiveRoutine(IRoutineRepository routines, IUnitOfWork unitOfWork, IRemindersRefresher reminders)
{
    public async Task<Result> ExecuteAsync(Guid routineId, CancellationToken cancellationToken = default)
    {
        var routine = await routines.GetByIdAsync(routineId, cancellationToken).ConfigureAwait(false);
        if (routine is null)
            return ApplicationErrors.RoutineNotFound;

        routine.Archive();
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await reminders.RefreshAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
