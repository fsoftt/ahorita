using Ahorita.App.Features.Celebration;
using Ahorita.App.Features.Editor;
using Ahorita.App.Features.Focus;
using Ahorita.App.Services;

namespace Ahorita.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(Routes.RoutineEditor, typeof(RoutineEditorPage));
        Routing.RegisterRoute(Routes.StepEditor, typeof(StepEditorPage));
        Routing.RegisterRoute(Routes.Focus, typeof(FocusPage));
        Routing.RegisterRoute(Routes.Celebration, typeof(CelebrationPage));
    }
}
