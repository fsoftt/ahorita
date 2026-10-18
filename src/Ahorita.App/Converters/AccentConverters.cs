using System.Globalization;
using Ahorita.App.Theming;
using Ahorita.Domain.Routines;

namespace Ahorita.App.Converters;

/// <summary>Acento → gradiente de la tarjeta.</summary>
public sealed class AccentToGradientConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is RoutineAccent accent ? AccentPalette.For(accent).Gradient : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Acento → color. Con <c>ConverterParameter=Soft</c> devuelve la versión translúcida.
/// Sin acento (p. ej. aún no hay rutina) devuelve transparente, nunca null: lo usan sombras.
/// </summary>
public sealed class AccentToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not RoutineAccent accent)
            return Colors.Transparent;

        var colors = AccentPalette.For(accent);
        return parameter is "Soft" ? colors.Soft : colors.End;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
