using Ahorita.Application.Abstractions;
using Ahorita.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Ahorita.Infrastructure.Persistence.Repositories;

internal sealed class SessionRepository(AhoritaDbContext db) : ISessionRepository
{
    public Task<RoutineSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Sessions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<RoutineSession?> GetActiveAsync(CancellationToken cancellationToken = default) =>
        db.Sessions
            .Where(s => s.Status == SessionStatus.Running || s.Status == SessionStatus.Paused)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<RoutineSession>> ListStartedBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default) =>
        await db.Sessions
            .Where(s => s.StartedAt >= from && s.StartedAt < to)
            .OrderBy(s => s.StartedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public void Add(RoutineSession session) => db.Sessions.Add(session);
}
