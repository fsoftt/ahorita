using Ahorita.App.Common;

namespace Ahorita.App.Features.Progress;

public partial class ProgressPage : AppPage
{
    public ProgressPage(ProgressViewModel viewModel)
        : base(viewModel) => InitializeComponent();
}
