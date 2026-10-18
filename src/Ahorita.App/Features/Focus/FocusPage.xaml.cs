using Ahorita.App.Common;
using Ahorita.App.Theming;

namespace Ahorita.App.Features.Focus;

public partial class FocusPage : AppPage
{
    public FocusPage(FocusViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
        viewModel.StepChanged += async (_, _) => await AnimateStepInAsync();
    }

    /// <summary>El siguiente paso entra deslizándose y el emoji rebota.</summary>
    private async Task AnimateStepInAsync()
    {
        if (Motion.IsReduced)
            return;

        StepContent.CancelAnimations();
        StepContent.TranslationX = 60;
        StepContent.Opacity = 0;
        StepEmojiLabel.Scale = 0.6;

        await Task.WhenAll(
            StepContent.TranslateToAsync(0, 0, Motion.Enter, Easing.CubicOut),
            StepContent.FadeToAsync(1, Motion.Enter, Easing.CubicOut),
            StepEmojiLabel.ScaleToAsync(1, Motion.Enter + 150, Easing.SpringOut));
    }
}
