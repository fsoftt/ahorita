using Ahorita.Application.Abstractions;
using Ahorita.Infrastructure.Persistence;
using Ahorita.Infrastructure.Persistence.Repositories;
using Ahorita.Infrastructure.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ahorita.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string databasePath)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddDbContext<AhoritaDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AhoritaDbContext>());
        services.AddScoped<IRoutineRepository, RoutineRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddSingleton<IStorageInitializer, DatabaseInitializer>();

        services.AddSingleton<IRoutineTemplateProvider, EmbeddedRoutineTemplateProvider>();

        return services;
    }
}
