using Ahorita.App.Services;

namespace Ahorita.App;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly PreferencesAppSettings _settings;

    public App(PreferencesAppSettings settings)
    {
        _settings = settings;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _settings.ApplyAll();
        return new Window(new AppShell());
    }
}
