using Ahorita.App.Common;

namespace Ahorita.App.Features.Donate;

public partial class DonatePage : AppPage
{
    public DonatePage(DonateViewModel viewModel)
        : base(viewModel) => InitializeComponent();
}
