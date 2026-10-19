using Ahorita.App.Common;

namespace Ahorita.App.Features.Settings;

public partial class SettingsPage : AppPage
{
    public SettingsPage(SettingsViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }

    // Tocar el texto de la fila alterna su interruptor, igual que tocar el interruptor.
    private void OnSettingRowTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Grid row && row.Children.OfType<Switch>().FirstOrDefault() is { } toggle)
            toggle.IsToggled = !toggle.IsToggled;
    }
}
