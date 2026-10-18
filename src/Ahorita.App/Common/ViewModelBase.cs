using Ahorita.App.Resources.Strings;
using Ahorita.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Ahorita.App.Common;

public abstract partial class ViewModelBase(IToastService toast, ILogger logger) : ObservableObject
{
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    protected IToastService Toast { get; } = toast;

    /// <summary>Se llama cada vez que la página aparece.</summary>
    public virtual Task OnAppearingAsync() => Task.CompletedTask;

    /// <summary>
    /// Ejecuta una acción de UI mostrando un mensaje amable si algo inesperado falla.
    /// La app nunca debe cerrarse por un error en una acción de la persona.
    /// </summary>
    protected async Task RunSafelyAsync(Func<Task> action)
    {
        try
        {
            IsBusy = true;
            await action();
        }
#pragma warning disable CA1031 // Última barrera de la UI: se registra y se avisa con tacto.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            logger.LogError(ex, "Falló una acción en {ViewModel}", GetType().Name);
            await Toast.ShowAsync(AppResources.Error_Generic);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
