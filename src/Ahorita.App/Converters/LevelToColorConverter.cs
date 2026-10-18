using System.Globalization;
using Ahorita.App.Theming;

namespace Ahorita.App.Converters;

/// <summary>
/// Intensidad del mapa de actividad (0–3) → tono de marca cada vez más fuerte.
/// Un día sin actividad es neutro, nunca rojo.
/// </summary>
public sealed class LevelToColorConverter : IValueConverter
{
    private static readonly float[] Alphas = [0f, 0.35f, 0.65f, 1f];

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var level = value is int l ? Math.Clamp(l, 0, 3) : 0;
        var isDark = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;

        if (level == 0)
            return ThemeResources.Color(isDark ? "SurfaceMutedDark" : "SurfaceMutedLight");

        return ThemeResources.Color(isDark ? "BrandDark" : "BrandLight").WithAlpha(Alphas[level]);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
