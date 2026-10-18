using Ahorita.App.Theming;

namespace Ahorita.App.Controls;

/// <summary>
/// Botón protagonista: se encoge al presionarlo, vibra al tocarlo y, opcionalmente,
/// "respira" despacio para invitar a empezar. Respeta <see cref="Motion.IsReduced"/>.
/// </summary>
public sealed class BigActionButton : Button
{
    public static readonly BindableProperty BreathesProperty = BindableProperty.Create(
        nameof(Breathes), typeof(bool), typeof(BigActionButton), false,
        propertyChanged: (bindable, _, _) => ((BigActionButton)bindable).UpdateBreathing());

    // Cada ciclo de "respiración" tiene un número; si cambia, el ciclo anterior se detiene.
    private int _breathCycle;
    private bool _isBreathing;

    public BigActionButton()
    {
        Pressed += OnPressed;
        Released += OnReleased;
        Clicked += (_, _) => Haptics.Tap();
        Loaded += (_, _) => UpdateBreathing();
        Unloaded += (_, _) => StopBreathing();
    }

    public bool Breathes
    {
        get => (bool)GetValue(BreathesProperty);
        set => SetValue(BreathesProperty, value);
    }

    private async void OnPressed(object? sender, EventArgs e)
    {
        StopBreathing();
        if (!Motion.IsReduced)
            await this.ScaleToAsync(Motion.PressedScale, Motion.Press, Easing.CubicOut);
    }

    private async void OnReleased(object? sender, EventArgs e)
    {
        if (!Motion.IsReduced)
            await this.ScaleToAsync(1, Motion.Release, Easing.SpringOut);

        UpdateBreathing();
    }

    private void UpdateBreathing()
    {
        StopBreathing();
        if (Breathes && IsLoaded && !Motion.IsReduced)
        {
            _isBreathing = true;
            _ = BreatheAsync(_breathCycle);
        }
    }

    private void StopBreathing()
    {
        if (!_isBreathing)
            return;

        _isBreathing = false;
        _breathCycle++;
        this.CancelAnimations();
        Scale = 1;
    }

    private async Task BreatheAsync(int cycle)
    {
        while (cycle == _breathCycle)
        {
            await this.ScaleToAsync(Motion.BreathScale, Motion.BreathHalfCycle, Easing.SinInOut);
            if (cycle != _breathCycle)
                break;

            await this.ScaleToAsync(1, Motion.BreathHalfCycle, Easing.SinInOut);
        }
    }
}
