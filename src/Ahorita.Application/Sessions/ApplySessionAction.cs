using Ahorita.Application.Abstractions;
using Ahorita.Domain.Common;
using Ahorita.Domain.Sessions;

namespace Ahorita.Application.Sessions;

/// <summary>
/// Una acción de la persona sobre la sesión en curso (Listo, Saltar, Después, Pausa…).
/// Se agrupan en un caso de uso porque comparten exactamente el mismo flujo:
/// cargar, aplicar la regla del dominio, guardar y reflejar en la notificación.
/// </summary>
public sealed class ApplySessionAction(
    ISessionRepository sessions,
    IUnitOfWork unitOfWork,
    IActiveRoutineNotifier notifier,
    TimeProvider time)
{
    public async Task<Result<SessionSnapshot>> ExecuteAsync(Guid sessionId, SessionAction action, CancellationToken cancellationToken = default)
    {
        var session = await sessions.GetByIdAsync(sessionId, cancellationToken).ConfigureAwait(false);
        if (session is null)
            return SessionErrors.NotFound;

        var now = time.GetUtcNow();
        var result = action switch
        {
            SessionAction.CompleteStep => session.CompleteCurrentStep(now),
            SessionAction.SkipStep => session.SkipCurrentStep(now),
            SessionAction.PostponeStep => session.PostponeCurrentStep(now),
            SessionAction.Pause => session.Pause(now),
            SessionAction.Resume => session.Resume(now),
            SessionAction.EndEarly => session.EndEarly(now),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        if (result.IsFailure)
            return result.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var snapshot = SessionSnapshot.From(session, now);
        await SessionNotifications.ReflectAsync(notifier, snapshot, cancellationToken).ConfigureAwait(false);
        return snapshot;
    }
}
