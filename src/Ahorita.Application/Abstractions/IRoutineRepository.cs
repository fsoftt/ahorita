using Ahorita.Domain.Routines;

namespace Ahorita.Application.Abstractions;

public interface IRoutineRepository
{
    Task<Routine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Rutinas no archivadas, con sus pasos.</summary>
    Task<IReadOnlyList<Routine>> ListActiveAsync(CancellationToken cancellationToken = default);

    void Add(Routine routine);
}
