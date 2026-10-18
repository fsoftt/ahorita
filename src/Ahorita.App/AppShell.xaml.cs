using Ahorita.App.Features.Celebration;
using Ahorita.App.Features.Editor;
using Ahorita.App.Features.Focus;
using Ahorita.App.Features.Onboarding;
using Ahorita.App.Features.Settings;
using Ahorita.App.Services;

namespace Ahorita.App;

public partial class AppShell : Shell
{
    private readonly DeepLinks _deepLinks;
    private readonly AppStartup _startup;
    private readonly IAppSettings _settings;
    private bool _ready;

    public AppShell(DeepLinks deepLinks, AppStartup startup, IAppSettings settings)
    {
        _deepLinks = deepLinks;
        _startup = startup;
        _settings = settings;
        InitializeComponent();

        Routing.RegisterRoute(Routes.RoutineEditor, typeof(RoutineEditorPage));
        Routing.RegisterRoute(Routes.StepEditor, typeof(StepEditorPage));
        Routing.RegisterRoute(Routes.Focus, typeof(FocusPage));
        Routing.RegisterRoute(Routes.Celebration, typeof(CelebrationPage));
        Routing.RegisterRoute(Routes.Settings, typeof(SettingsPage));
        Routing.RegisterRoute(Routes.Onboarding, typeof(OnboardingPage));
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);
        if (_ready)
            return;

        _ready = true;
        _startup.RunOnce();

        // Un enlace desde una notificación tiene prioridad sobre la bienvenida.
        if (!_deepLinks.ShellReady() && !_settings.OnboardingCompleted)
            Dispatcher.Dispatch(async () => await GoToAsync(Routes.Onboarding));
    }
}
