using Ahorita.App.Common;

namespace Ahorita.App.Features.Editor;

public partial class RoutineEditorPage : AppPage
{
    public RoutineEditorPage(RoutineEditorViewModel viewModel)
        : base(viewModel) => InitializeComponent();
}
