using Ahorita.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Ahorita.Application.Reminders;

internal sealed partial class RemindersRefresher(
    IRoutineRepository routines,
    IReminderScheduler scheduler,
    TimeProvider time,
    ILogger<RemindersRefresher> logger) : IRemindersRefresher
{
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var active = await routines.ListActiveAsync(cancellationToken).ConfigureAwait(false);
            var plan = ReminderPlanner.Plan(active, time.GetUtcNow(), time.LocalTimeZone);
            await scheduler.ReplaceAllAsync(plan, cancellationToken).ConfigureAwait(false);
        }
#pragma warning disable CA1031 // Programar avisos es secundario: nunca debe romper el guardado.
        catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
        {
            LogRefreshFailed(logger, ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "No se pudieron reprogramar los recordatorios")]
    private static partial void LogRefreshFailed(ILogger logger, Exception exception);
}
