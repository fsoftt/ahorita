using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ahorita.Infrastructure.Persistence;

/// <summary>Solo para <c>dotnet ef migrations</c>; la app configura el contexto en <see cref="DependencyInjection"/>.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AhoritaDbContext>
{
    public AhoritaDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AhoritaDbContext>().UseSqlite("Data Source=ahorita-design.db").Options);
}
