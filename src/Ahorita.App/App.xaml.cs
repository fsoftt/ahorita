using Ahorita.App.Services;

namespace Ahorita.App;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly PreferencesAppSettings _settings;
    private readonly IServiceProvider _services;

    public App(PreferencesAppSettings settings, IServiceProvider services)
    {
        _settings = settings;
        _services = services;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _settings.ApplyAll();
        return new Window(_services.GetRequiredService<AppShell>());
    }
}
