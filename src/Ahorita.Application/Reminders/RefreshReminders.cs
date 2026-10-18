using Ahorita.Application.Abstractions;

namespace Ahorita.Application.Reminders;

/// <summary>Punto de entrada para la plataforma (arranque, reinicio, alarma disparada, cambio de hora).</summary>
public sealed class RefreshReminders(IRemindersRefresher refresher)
{
    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await refresher.RefreshAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
