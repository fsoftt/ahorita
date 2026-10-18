namespace Ahorita.App.Services;

/// <summary>
/// "Apoyar con anuncios": apagado por defecto. Quien quiera ayudar lo enciende y aparece
/// un banner pequeño al pie de las pantallas principales.
/// </summary>
public sealed class SupportAdsPreference(IPreferences preferences)
{
    private const string PreferenceKey = "SupportWithAds";

    public event EventHandler? Changed;

    public bool IsEnabled
    {
        get => preferences.Get(PreferenceKey, false);
        set
        {
            if (value == IsEnabled)
                return;

            preferences.Set(PreferenceKey, value);
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
