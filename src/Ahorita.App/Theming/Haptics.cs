namespace Ahorita.App.Theming;

/// <summary>Háptica segura: si el dispositivo no la soporta, simplemente no vibra.</summary>
public static class Haptics
{
    /// <summary>Se puede apagar en Ajustes.</summary>
    public static bool IsEnabled { get; set; } = true;

    public static void Tap() => Perform(HapticFeedbackType.Click);

    public static void Confirm() => Perform(HapticFeedbackType.LongPress);

    /// <summary>Patrón "¡tada!": tres pulsos crecientes, distinto de un toque normal.</summary>
    public static async Task CelebrateAsync()
    {
        Perform(HapticFeedbackType.Click);
        await Task.Delay(110);
        Perform(HapticFeedbackType.Click);
        await Task.Delay(140);
        Perform(HapticFeedbackType.LongPress);
    }

    private static void Perform(HapticFeedbackType type)
    {
        if (!IsEnabled)
            return;

        try
        {
            if (HapticFeedback.Default.IsSupported)
                HapticFeedback.Default.Perform(type);
        }
        catch (FeatureNotSupportedException)
        {
            // Sin motor de vibración: no hay nada que hacer.
        }
    }
}
