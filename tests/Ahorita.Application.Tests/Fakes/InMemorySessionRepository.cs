using Ahorita.Application.Abstractions;
using Ahorita.Domain.Sessions;

namespace Ahorita.Application.Tests.Fakes;

internal sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly List<RoutineSession> _sessions = [];

    public IReadOnlyList<RoutineSession> All => _sessions;

    public Task<RoutineSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_sessions.FirstOrDefault(s => s.Id == id));

    public Task<RoutineSession?> GetActiveAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_sessions.LastOrDefault(s => s.IsActive));

    public Task<IReadOnlyList<RoutineSession>> ListStartedBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RoutineSession>>(_sessions.Where(s => s.StartedAt >= from && s.StartedAt < to).ToList());

    public void Add(RoutineSession session) => _sessions.Add(session);
}
