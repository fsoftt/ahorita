using Ahorita.Application.Abstractions;
using Ahorita.Application.Reminders;
using Ahorita.Application.Sessions;
using Microsoft.Extensions.Logging;

namespace Ahorita.App.Services;

/// <summary>Tareas de arranque en segundo plano: no bloquean la primera pantalla.</summary>
public sealed partial class AppStartup(IUseCaseExecutor useCases, ILogger<AppStartup> logger)
{
    private int _started;

    public void RunOnce()
    {
        if (Interlocked.Exchange(ref _started, 1) == 1)
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                await useCases.ExecuteAsync((RefreshReminders useCase) => useCase.ExecuteAsync()).ConfigureAwait(false);
                await useCases.ExecuteAsync((RestoreSessionNotification useCase) => useCase.ExecuteAsync()).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Arranque en segundo plano: se registra y la app sigue.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                LogFailed(logger, ex);
            }
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Falló una tarea de arranque")]
    private static partial void LogFailed(ILogger logger, Exception exception);
}
