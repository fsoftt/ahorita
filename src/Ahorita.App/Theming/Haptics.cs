namespace Ahorita.App.Theming;

/// <summary>Háptica segura: si el dispositivo no la soporta, simplemente no vibra.</summary>
public static class Haptics
{
    public static void Tap() => Perform(HapticFeedbackType.Click);

    public static void Confirm() => Perform(HapticFeedbackType.LongPress);

    private static void Perform(HapticFeedbackType type)
    {
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
