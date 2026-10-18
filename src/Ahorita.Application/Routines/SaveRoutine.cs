using Ahorita.Application.Abstractions;
using Ahorita.Domain.Common;
using Ahorita.Domain.Routines;

namespace Ahorita.Application.Routines;

/// <summary>
/// Crea o actualiza una rutina completa a partir del borrador del editor. Todo o nada:
/// si alguna regla falla, no se guarda ningún cambio.
/// </summary>
public sealed class SaveRoutine(
    IRoutineRepository routines,
    IUnitOfWork unitOfWork,
    IRemindersRefresher reminders,
    TimeProvider time)
{
    public async Task<Result<Guid>> ExecuteAsync(RoutineDraft draft, CancellationToken cancellationToken = default)
    {
        var schedule = RoutineSchedule.Create(draft.Days, draft.StartTime, draft.ReminderLeadTime);
        if (schedule.IsFailure)
            return schedule.Error;

        Routine routine;
        if (draft.Id is { } id)
        {
            var existing = await routines.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (existing is null)
                return ApplicationErrors.RoutineNotFound;

            routine = existing;
            var identity = Combine(routine.Rename(draft.Name), routine.ChangeAppearance(draft.Emoji, draft.Accent));
            if (identity.IsFailure)
                return identity.Error;

            routine.Reschedule(schedule.Value);
        }
        else
        {
            var created = Routine.Create(draft.Name, draft.Emoji, draft.Accent, schedule.Value, time.GetUtcNow());
            if (created.IsFailure)
                return created.Error;

            routine = created.Value;
            routines.Add(routine);
        }

        var steps = SyncSteps(routine, draft.Steps);
        if (steps.IsFailure)
            return steps.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await reminders.RefreshAsync(cancellationToken).ConfigureAwait(false);
        return routine.Id;
    }

    private static Result SyncSteps(Routine routine, IReadOnlyList<StepDraft> drafts)
    {
        var keptIds = drafts.Where(d => d.Id is not null).Select(d => d.Id!.Value).ToHashSet();
        foreach (var removed in routine.Steps.Where(s => !keptIds.Contains(s.Id)).ToArray())
            routine.RemoveStep(removed.Id);

        var finalOrder = new List<Guid>(drafts.Count);
        foreach (var draft in drafts)
        {
            if (draft.Id is { } stepId)
            {
                var updated = routine.UpdateStep(stepId, draft.Title, draft.Emoji, draft.Duration, draft.IsOptional);
                if (updated.IsFailure)
                    return updated;

                finalOrder.Add(stepId);
            }
            else
            {
                var added = routine.AddStep(draft.Title, draft.Emoji, draft.Duration, draft.IsOptional);
                if (added.IsFailure)
                    return added.Error;

                finalOrder.Add(added.Value.Id);
            }
        }

        return routine.ReorderSteps(finalOrder);
    }

    private static Result Combine(params Result[] results) =>
        results.FirstOrDefault(r => r.IsFailure) ?? Result.Success();
}
