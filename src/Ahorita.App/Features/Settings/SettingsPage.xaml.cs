using Ahorita.App.Common;

namespace Ahorita.App.Features.Settings;

public partial class SettingsPage : AppPage
{
    public SettingsPage(SettingsViewModel viewModel)
        : base(viewModel) => InitializeComponent();
}
