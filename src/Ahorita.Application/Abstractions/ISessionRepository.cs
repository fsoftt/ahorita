using Ahorita.Domain.Sessions;

namespace Ahorita.Application.Abstractions;

public interface ISessionRepository
{
    Task<RoutineSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>La sesión en curso o en pausa, si existe (como máximo hay una).</summary>
    Task<RoutineSession?> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Sesiones iniciadas en [<paramref name="from"/>, <paramref name="to"/>).</summary>
    Task<IReadOnlyList<RoutineSession>> ListStartedBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);

    void Add(RoutineSession session);
}
