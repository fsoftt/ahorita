using System.Globalization;
using Ahorita.App.Resources.Strings;

namespace Ahorita.App.Services;

public enum AppLanguage
{
    /// <summary>Sigue el idioma del teléfono (español si no es uno soportado).</summary>
    System,
    Spanish,
    English,
}

/// <summary>
/// Idioma de la interfaz. Se aplica al arrancar el proceso (también cuando lo despierta
/// una alarma) para que textos, fechas, plantillas y notificaciones coincidan.
/// </summary>
public static class Localization
{
    public const string PreferenceKey = "Language";

    private static readonly CultureInfo SystemCulture = CultureInfo.CurrentUICulture;

    public static AppLanguage Read(IPreferences preferences) =>
        Enum.TryParse<AppLanguage>(preferences.Get(PreferenceKey, nameof(AppLanguage.System)), out var language)
            ? language
            : AppLanguage.System;

    public static void Apply(AppLanguage language)
    {
        var culture = language switch
        {
            AppLanguage.Spanish => CultureInfo.GetCultureInfo(Prefer(SystemCulture, "es")),
            AppLanguage.English => CultureInfo.GetCultureInfo(Prefer(SystemCulture, "en")),
            _ => SystemCulture,
        };

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        AppResources.Culture = culture;
    }

    /// <summary>Conserva la región del teléfono si coincide el idioma (es-MX, en-GB…).</summary>
    private static string Prefer(CultureInfo system, string language) =>
        system.TwoLetterISOLanguageName == language ? system.Name : language;
}
