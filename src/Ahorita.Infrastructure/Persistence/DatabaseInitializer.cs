using Ahorita.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ahorita.Infrastructure.Persistence;

/// <summary>Aplica las migraciones pendientes una sola vez por ejecución de la app.</summary>
internal sealed class DatabaseInitializer(IServiceScopeFactory scopeFactory) : IStorageInitializer
{
    private readonly Lazy<Task> _initialization = new(() => MigrateAsync(scopeFactory));

    public Task EnsureInitializedAsync() => _initialization.Value;

    private static async Task MigrateAsync(IServiceScopeFactory scopeFactory)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AhoritaDbContext>();
        await db.Database.MigrateAsync().ConfigureAwait(false);
    }
}
