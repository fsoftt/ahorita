using Ahorita.Application.Abstractions;
using Ahorita.Domain.Routines;
using Microsoft.EntityFrameworkCore;

namespace Ahorita.Infrastructure.Persistence;

public sealed class AhoritaDbContext(DbContextOptions<AhoritaDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Routine> Routines => Set<Routine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AhoritaDbContext).Assembly);

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
}
