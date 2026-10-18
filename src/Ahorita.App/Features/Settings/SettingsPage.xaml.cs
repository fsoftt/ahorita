using Ahorita.App.Common;

namespace Ahorita.App.Features.Settings;

public partial class SettingsPage : AppPage
{
    private readonly SettingsViewModel _viewModel;
    private Window? _window;

    public SettingsPage(SettingsViewModel viewModel)
        : base(viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Volver de los ajustes de Android no dispara OnAppearing: se escucha al volver la app.
        _window = Window;
        if (_window is not null)
            _window.Resumed += OnResumed;
    }

    protected override void OnDisappearing()
    {
        if (_window is not null)
            _window.Resumed -= OnResumed;

        _window = null;
        base.OnDisappearing();
    }

    // Tocar el texto de la fila alterna su interruptor, igual que tocar el interruptor.
    private void OnSettingRowTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Grid row && row.Children.OfType<Switch>().FirstOrDefault() is { } toggle)
            toggle.IsToggled = !toggle.IsToggled;
    }

    private async void OnResumed(object? sender, EventArgs e) => await _viewModel.OnAppearingAsync();
}
