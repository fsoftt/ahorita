using Ahorita.Application.Abstractions;
using Ahorita.Domain.Common;
using Ahorita.Domain.Sessions;

namespace Ahorita.Application.Sessions;

public sealed class GetSession(ISessionRepository sessions, TimeProvider time)
{
    public async Task<Result<SessionSnapshot>> ExecuteAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await sessions.GetByIdAsync(sessionId, cancellationToken).ConfigureAwait(false);
        return session is null ? SessionErrors.NotFound : SessionSnapshot.From(session, time.GetUtcNow());
    }
}

public sealed class GetActiveSession(ISessionRepository sessions, TimeProvider time)
{
    public async Task<SessionSnapshot?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var session = await sessions.GetActiveAsync(cancellationToken).ConfigureAwait(false);
        return session is null ? null : SessionSnapshot.From(session, time.GetUtcNow());
    }
}
