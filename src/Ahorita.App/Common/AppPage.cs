using Ahorita.App.Theming;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

namespace Ahorita.App.Common;

/// <summary>
/// Página base: sin barra de navegación (cada pantalla tiene su propio título grande),
/// respeta el área segura, barra de estado del color del fondo y carga del ViewModel al aparecer y al volver a la app.
/// </summary>
public abstract class AppPage : ContentPage
{
    private Window? _window;

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

        // Volver de los ajustes de Android no dispara OnAppearing: se escucha al volver la app.
        // En una página recién empujada (el editor) Window aún es null al aparecer: se usa la de la app.
        _window = Window ?? Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        if (_window is not null)
            _window.Resumed += OnResumed;

        await ViewModel.OnAppearingAsync();
    }

    protected override void OnDisappearing()
    {
        if (_window is not null)
            _window.Resumed -= OnResumed;

        _window = null;
        base.OnDisappearing();
        ViewModel.OnDisappearing();
    }

    private async void OnResumed(object? sender, EventArgs e) => await ViewModel.OnResumedAsync();

    private static StatusBarBehavior CreateStatusBarBehavior()
    {
        var behavior = new StatusBarBehavior();
        ThemeResources.SetThemeColor(behavior, StatusBarBehavior.StatusBarColorProperty, "PageBackgroundLight", "PageBackgroundDark");
        behavior.SetAppTheme(StatusBarBehavior.StatusBarStyleProperty, StatusBarStyle.DarkContent, StatusBarStyle.LightContent);
        return behavior;
    }
}
