using Ahorita.Application.Abstractions;
using Ahorita.Domain.Common;
using Ahorita.Domain.Sessions;

namespace Ahorita.Application.Sessions;

/// <summary>
/// Empieza (o retoma) una rutina. Si ya hay otra en curso, esa termina antes:
/// lo que se hizo cuenta y nunca se pierde.
/// </summary>
public sealed class StartSession(
    IRoutineRepository routines,
    ISessionRepository sessions,
    IUnitOfWork unitOfWork,
    IActiveRoutineNotifier notifier,
    TimeProvider time)
{
    public async Task<Result<SessionSnapshot>> ExecuteAsync(Guid routineId, CancellationToken cancellationToken = default)
    {
        var now = time.GetUtcNow();
        var active = await sessions.GetActiveAsync(cancellationToken).ConfigureAwait(false);

        if (active is not null && active.RoutineId == routineId)
            return SessionSnapshot.From(active, now);

        var routine = await routines.GetByIdAsync(routineId, cancellationToken).ConfigureAwait(false);
        if (routine is null || routine.IsArchived)
            return ApplicationErrors.RoutineNotFound;

        var started = RoutineSession.Start(routine, now);
        if (started.IsFailure)
            return started.Error;

        active?.EndEarly(now);
        sessions.Add(started.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var snapshot = SessionSnapshot.From(started.Value, now);
        await SessionNotifications.ReflectAsync(notifier, snapshot, cancellationToken).ConfigureAwait(false);
        return snapshot;
    }
}
