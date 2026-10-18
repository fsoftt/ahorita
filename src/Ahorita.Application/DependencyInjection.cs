using Ahorita.Application.Abstractions;
using Ahorita.Application.Routines;
using Microsoft.Extensions.DependencyInjection;

namespace Ahorita.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IUseCaseExecutor, ScopedUseCaseExecutor>();

        services.AddScoped<GetTodayOverview>();
        services.AddScoped<ListRoutines>();
        services.AddScoped<ListRoutineTemplates>();
        services.AddScoped<CreateRoutineFromTemplate>();

        return services;
    }
}
