namespace Ahorita.App.Services;

public enum ThemePreference
{
    System,
    Light,
    Dark,
}

/// <summary>Preferencias de la persona (se guardan en el dispositivo).</summary>
public interface IAppSettings
{
    /// <summary>Sin animaciones, confeti ni gradientes llamativos.</summary>
    bool LowStimulus { get; set; }

    bool Celebrations { get; set; }

    bool HapticsEnabled { get; set; }

    bool ReadStepsAloud { get; set; }

    ThemePreference Theme { get; set; }

    bool OnboardingCompleted { get; set; }
}
