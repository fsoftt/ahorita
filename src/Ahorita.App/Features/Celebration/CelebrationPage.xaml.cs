using Ahorita.App.Common;
using Ahorita.App.Theming;

namespace Ahorita.App.Features.Celebration;

public partial class CelebrationPage : AppPage
{
    public CelebrationPage(CelebrationViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
        viewModel.Ready += async (_, _) => await CelebrateAsync(viewModel);
    }

    private async Task CelebrateAsync(CelebrationViewModel viewModel)
    {
        _ = Haptics.CelebrateAsync();
        if (!viewModel.ShowsEffects || Motion.IsReduced)
            return;

        Confetti.Burst();
        EmojiLabel.Scale = 0.3;
        await EmojiLabel.ScaleToAsync(1, 600, Easing.SpringOut);
    }
}
