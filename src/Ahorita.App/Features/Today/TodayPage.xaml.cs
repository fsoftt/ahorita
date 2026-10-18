using Ahorita.App.Common;

namespace Ahorita.App.Features.Today;

public partial class TodayPage : AppPage
{
    public TodayPage(TodayViewModel viewModel)
        : base(viewModel) => InitializeComponent();
}
