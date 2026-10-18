using Ahorita.App.Theming;

namespace Ahorita.App.Services;

/// <summary>
/// Preferencias sobre <see cref="IPreferences"/>. Al cambiar las que afectan la UI
/// se aplican al momento (tema, movimiento, háptica).
/// </summary>
public sealed class PreferencesAppSettings(IPreferences preferences) : IAppSettings
{
    public bool LowStimulus
    {
        get => preferences.Get(nameof(LowStimulus), false);
        set
        {
            preferences.Set(nameof(LowStimulus), value);
            Motion.LowStimulus = value;
        }
    }

    public bool Celebrations
    {
        get => preferences.Get(nameof(Celebrations), true);
        set => preferences.Set(nameof(Celebrations), value);
    }

    public bool HapticsEnabled
    {
        get => preferences.Get(nameof(HapticsEnabled), true);
        set
        {
            preferences.Set(nameof(HapticsEnabled), value);
            Haptics.IsEnabled = value;
        }
    }

    public bool ReadStepsAloud
    {
        get => preferences.Get(nameof(ReadStepsAloud), false);
        set => preferences.Set(nameof(ReadStepsAloud), value);
    }

    public ThemePreference Theme
    {
        get => Enum.TryParse<ThemePreference>(preferences.Get(nameof(Theme), nameof(ThemePreference.System)), out var theme)
            ? theme
            : ThemePreference.System;
        set
        {
            preferences.Set(nameof(Theme), value.ToString());
            ApplyTheme(value);
        }
    }

    public AppLanguage Language
    {
        get => Localization.Read(preferences);
        set
        {
            preferences.Set(Localization.PreferenceKey, value.ToString());
            Localization.Apply(value);
        }
    }

    public bool OnboardingCompleted
    {
        get => preferences.Get(nameof(OnboardingCompleted), false);
        set => preferences.Set(nameof(OnboardingCompleted), value);
    }

    /// <summary>Lleva las preferencias guardadas al estado global de la UI al arrancar.</summary>
    public void ApplyAll()
    {
        Motion.LowStimulus = LowStimulus;
        Haptics.IsEnabled = HapticsEnabled;
        ApplyTheme(Theme);
    }

    private static void ApplyTheme(ThemePreference theme)
    {
        if (Microsoft.Maui.Controls.Application.Current is not { } app)
            return;

        app.UserAppTheme = theme switch
        {
            ThemePreference.Light => AppTheme.Light,
            ThemePreference.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified,
        };
    }
}
