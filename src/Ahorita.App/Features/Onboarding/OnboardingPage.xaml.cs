using Ahorita.App.Common;

namespace Ahorita.App.Features.Onboarding;

public partial class OnboardingPage : AppPage
{
    public OnboardingPage(OnboardingViewModel viewModel)
        : base(viewModel) => InitializeComponent();
}
