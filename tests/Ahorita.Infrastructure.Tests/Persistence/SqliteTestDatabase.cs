using Ahorita.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ahorita.Infrastructure.Tests.Persistence;

/// <summary>SQLite real en memoria con las migraciones aplicadas, igual que la app.</summary>
public abstract class SqliteTestDatabase : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private DbContextOptions<AhoritaDbContext> _options = null!;

    public async ValueTask InitializeAsync()
    {
        await _connection.OpenAsync();
        _options = new DbContextOptionsBuilder<AhoritaDbContext>().UseSqlite(_connection).Options;

        await using var db = new AhoritaDbContext(_options);
        await db.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected AhoritaDbContext NewContext() => new(_options);
}
