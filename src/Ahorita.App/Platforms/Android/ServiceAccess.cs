using Ahorita.Application.Abstractions;
using Android.Content;

namespace Ahorita.App.Droid;

/// <summary>
/// Acceso a los servicios de la app desde receivers de Android, que el sistema crea
/// fuera del contenedor de DI.
/// </summary>
internal static class ServiceAccess
{
    public static IServiceProvider? Services => IPlatformApplication.Current?.Services;

    public static IUseCaseExecutor? UseCases => Services?.GetService<IUseCaseExecutor>();

    /// <summary>
    /// Ejecuta trabajo asíncrono desde un BroadcastReceiver sin que Android mate el proceso
    /// a mitad (goAsync), con un límite de tiempo prudente.
    /// </summary>
    public static void RunInBackground(BroadcastReceiver receiver, Func<Task> work)
    {
        var pending = receiver.GoAsync();
        _ = Task.Run(async () =>
        {
            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                await work().WaitAsync(timeout.Token).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Un receiver nunca debe tumbar la app.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                Android.Util.Log.Error("Ahorita", ex.ToString());
            }
            finally
            {
                pending?.Finish();
            }
        });
    }
}
