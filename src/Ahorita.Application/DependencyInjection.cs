using Ahorita.Application.Abstractions;
using Ahorita.Application.Reminders;
using Ahorita.Application.Routines;
using Ahorita.Application.Sessions;
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
        services.AddScoped<GetRoutineForEditing>();
        services.AddScoped<SaveRoutine>();
        services.AddScoped<ArchiveRoutine>();

        services.AddScoped<StartSession>();
        services.AddScoped<ApplySessionAction>();
        services.AddScoped<GetSession>();
        services.AddScoped<GetActiveSession>();

        services.AddScoped<IRemindersRefresher, RemindersRefresher>();
        services.AddScoped<RefreshReminders>();

        return services;
    }
}
