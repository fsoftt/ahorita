using Ahorita.Application.Abstractions;
using Ahorita.Domain.Routines;
using Microsoft.EntityFrameworkCore;

namespace Ahorita.Infrastructure.Persistence.Repositories;

internal sealed class RoutineRepository(AhoritaDbContext db) : IRoutineRepository
{
    public Task<Routine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Routines.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Routine>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await db.Routines
            .Where(r => !r.IsArchived)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public void Add(Routine routine) => db.Routines.Add(routine);
}
