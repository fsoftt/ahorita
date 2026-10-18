using Ahorita.Application.Abstractions;
using Ahorita.Domain.Common;
using Ahorita.Domain.Routines;

namespace Ahorita.Application.Routines;

/// <summary>Crea una rutina editable a partir de una plantilla: el camino de menos de 60 segundos.</summary>
public sealed class CreateRoutineFromTemplate(
    IRoutineTemplateProvider templates,
    IRoutineRepository routines,
    IUnitOfWork unitOfWork,
    IRemindersRefresher reminders,
    TimeProvider time)
{
    public async Task<Result<RoutineSummary>> ExecuteAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var template = templates.GetAll().FirstOrDefault(t => t.Key == templateKey);
        if (template is null)
            return ApplicationErrors.TemplateNotFound;

        var schedule = RoutineSchedule.Create(template.Days, template.StartTime);
        if (schedule.IsFailure)
            return schedule.Error;

        var created = Routine.Create(template.Name, template.Emoji, template.Accent, schedule.Value, time.GetUtcNow());
        if (created.IsFailure)
            return created.Error;

        var routine = created.Value;
        foreach (var step in template.Steps)
        {
            var added = routine.AddStep(step.Title, step.Emoji, step.Duration, step.IsOptional);
            if (added.IsFailure)
                return added.Error;
        }

        routines.Add(routine);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await reminders.RefreshAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return RoutineSummary.From(routine);
    }
}
