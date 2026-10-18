using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

namespace Ahorita.App.Common;

/// <summary>
/// Página base: sin barra de navegación (cada pantalla tiene su propio título grande),
/// barra de estado del color del fondo y carga del ViewModel al aparecer.
/// </summary>
public abstract class AppPage : ContentPage
{
    protected AppPage(ViewModelBase viewModel)
    {
        BindingContext = ViewModel = viewModel;
        Shell.SetNavBarIsVisible(this, false);
        Behaviors.Add(CreateStatusBarBehavior());
    }

    protected ViewModelBase ViewModel { get; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.OnAppearingAsync();
    }

    private static StatusBarBehavior CreateStatusBarBehavior()
    {
        var behavior = new StatusBarBehavior();
        behavior.SetAppThemeColor(StatusBarBehavior.StatusBarColorProperty, Resource<Color>("PageBackgroundLight"), Resource<Color>("PageBackgroundDark"));
        behavior.SetAppTheme(StatusBarBehavior.StatusBarStyleProperty, StatusBarStyle.DarkContent, StatusBarStyle.LightContent);
        return behavior;
    }

    private static T Resource<T>(string key) =>
        Microsoft.Maui.Controls.Application.Current!.Resources.TryGetValue(key, out var value) && value is T typed
            ? typed
            : throw new InvalidOperationException($"Falta el recurso '{key}'.");
}
