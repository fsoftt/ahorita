using Ahorita.Application.Abstractions;
using Ahorita.Domain.Routines;

namespace Ahorita.Application.Tests.Fakes;

internal sealed class InMemoryRoutineRepository : IRoutineRepository, IUnitOfWork
{
    private readonly List<Routine> _routines = [];

    public int SaveCount { get; private set; }

    public IReadOnlyList<Routine> All => _routines;

    public Task<Routine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_routines.FirstOrDefault(r => r.Id == id));

    public Task<IReadOnlyList<Routine>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Routine>>(_routines.Where(r => !r.IsArchived).ToList());

    public void Add(Routine routine) => _routines.Add(routine);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
