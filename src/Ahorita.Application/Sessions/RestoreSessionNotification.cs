using Ahorita.Application.Abstractions;

namespace Ahorita.Application.Sessions;

/// <summary>Vuelve a mostrar la notificación de la rutina en curso (tras reiniciar o abrir la app).</summary>
public sealed class RestoreSessionNotification(ISessionRepository sessions, IActiveRoutineNotifier notifier, TimeProvider time)
{
    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var session = await sessions.GetActiveAsync(cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            await notifier.ClearAsync(cancellationToken).ConfigureAwait(false);
            return false;
        }

        await notifier.ShowAsync(SessionSnapshot.From(session, time.GetUtcNow()), cancellationToken).ConfigureAwait(false);
        return true;
    }
}
