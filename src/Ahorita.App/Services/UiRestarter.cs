namespace Ahorita.App.Services;

/// <summary>
/// Reconstruye la interfaz (p. ej. tras cambiar de idioma) sin cerrar la app:
/// las páginas leen sus textos al crearse, así que se crea un Shell nuevo.
/// </summary>
public sealed class UiRestarter(IServiceProvider services)
{
    /// <param name="route">Pantalla a la que volver, para no perder el contexto.</param>
    public void Restart(string? route = null) => MainThread.BeginInvokeOnMainThread(async () =>
    {
        if (Microsoft.Maui.Controls.Application.Current?.Windows is not [{ } window, ..])
            return;

        var shell = services.GetRequiredService<AppShell>();
        window.Page = shell;
        if (route is not null)
            await shell.GoToAsync(route, animate: false);

#if ANDROID
        // Los nombres de los canales de notificación también cambian de idioma.
        Droid.Notifications.AhoritaNotifications.EnsureChannels(global::Android.App.Application.Context);
#endif
    });
}
