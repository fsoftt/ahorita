namespace Ahorita.App.Services;

/// <summary>Navegación desacoplada de Shell para que los ViewModels no dependan de la UI.</summary>
public interface INavigationService
{
    Task GoToAsync(string route, IDictionary<string, object>? parameters = null);

    Task GoBackAsync();
}
