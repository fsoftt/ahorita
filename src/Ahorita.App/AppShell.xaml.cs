using Ahorita.App.Features.Celebration;
using Ahorita.App.Features.Editor;
using Ahorita.App.Features.Focus;
using Ahorita.App.Services;

namespace Ahorita.App;

public partial class AppShell : Shell
{
    private readonly DeepLinks _deepLinks;
    private readonly AppStartup _startup;
    private bool _ready;

    public AppShell(DeepLinks deepLinks, AppStartup startup)
    {
        _deepLinks = deepLinks;
        _startup = startup;
        InitializeComponent();

        Routing.RegisterRoute(Routes.RoutineEditor, typeof(RoutineEditorPage));
        Routing.RegisterRoute(Routes.StepEditor, typeof(StepEditorPage));
        Routing.RegisterRoute(Routes.Focus, typeof(FocusPage));
        Routing.RegisterRoute(Routes.Celebration, typeof(CelebrationPage));
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);
        if (_ready)
            return;

        _ready = true;
        _startup.RunOnce();
        _deepLinks.ShellReady();
    }
}
