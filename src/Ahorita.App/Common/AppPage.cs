using Ahorita.App.Theming;
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
        ThemeResources.SetThemeColor(behavior, StatusBarBehavior.StatusBarColorProperty, "PageBackgroundLight", "PageBackgroundDark");
        behavior.SetAppTheme(StatusBarBehavior.StatusBarStyleProperty, StatusBarStyle.DarkContent, StatusBarStyle.LightContent);
        return behavior;
    }
}
