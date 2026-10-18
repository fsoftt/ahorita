using Ahorita.App.Theming;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

namespace Ahorita.App.Common;

/// <summary>
/// Página base: sin barra de navegación (cada pantalla tiene su propio título grande),
/// respeta el área segura, barra de estado del color del fondo y carga del ViewModel al aparecer.
/// </summary>
public abstract class AppPage : ContentPage
{
    protected AppPage(ViewModelBase viewModel)
    {
        BindingContext = ViewModel = viewModel;
        Shell.SetNavBarIsVisible(this, false);

        // Android 15+ dibuja de borde a borde: respetamos barras del sistema, recortes y teclado.
        SafeAreaEdges = SafeAreaEdges.All;
        Behaviors.Add(CreateStatusBarBehavior());
    }

    protected ViewModelBase ViewModel { get; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.OnAppearingAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ViewModel.OnDisappearing();
    }

    private static StatusBarBehavior CreateStatusBarBehavior()
    {
        var behavior = new StatusBarBehavior();
        ThemeResources.SetThemeColor(behavior, StatusBarBehavior.StatusBarColorProperty, "PageBackgroundLight", "PageBackgroundDark");
        behavior.SetAppTheme(StatusBarBehavior.StatusBarStyleProperty, StatusBarStyle.DarkContent, StatusBarStyle.LightContent);
        return behavior;
    }
}
