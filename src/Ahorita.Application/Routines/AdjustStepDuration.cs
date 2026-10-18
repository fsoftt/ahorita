using Ahorita.Application.Abstractions;
using Ahorita.Domain.Common;
using Ahorita.Domain.Routines;

namespace Ahorita.Application.Routines;

/// <summary>Acepta una sugerencia de duración ("¿Ajustamos?") sin pasar por el editor.</summary>
public sealed class AdjustStepDuration(IRoutineRepository routines, IUnitOfWork unitOfWork)
{
    public async Task<Result> ExecuteAsync(Guid routineId, Guid stepId, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        var routine = await routines.GetByIdAsync(routineId, cancellationToken).ConfigureAwait(false);
        if (routine is null)
            return ApplicationErrors.RoutineNotFound;

        if (routine.Steps.FirstOrDefault(s => s.Id == stepId) is not { } step)
            return RoutineErrors.StepNotFound;

        var updated = routine.UpdateStep(step.Id, step.Title, step.Emoji, duration, step.IsOptional);
        if (updated.IsFailure)
            return updated;

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
