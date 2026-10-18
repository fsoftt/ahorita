using Ahorita.Domain.Routines;

namespace Ahorita.App.Theming;

/// <summary>
/// Tonos de cada acento de rutina. Son claros y saturados a propósito: el texto encima
/// siempre es tinta oscura (<c>OnAccent</c>), lo que mantiene contraste AA en ambos temas.
/// </summary>
public static class AccentPalette
{
    public static AccentColors For(RoutineAccent accent) => accent switch
    {
        RoutineAccent.Tangerine => new(Color.FromArgb("#FFC46B"), Color.FromArgb("#FF8A5C")),
        RoutineAccent.Indigo => new(Color.FromArgb("#B9C2FF"), Color.FromArgb("#8E98FA")),
        RoutineAccent.Mint => new(Color.FromArgb("#8EF0C6"), Color.FromArgb("#3ED6A0")),
        RoutineAccent.Raspberry => new(Color.FromArgb("#FFB8DD"), Color.FromArgb("#F57CBC")),
        RoutineAccent.Lemon => new(Color.FromArgb("#FFEFA0"), Color.FromArgb("#FAD02C")),
        RoutineAccent.Sky => new(Color.FromArgb("#A6E3FF"), Color.FromArgb("#4FC3F7")),
        _ => throw new ArgumentOutOfRangeException(nameof(accent), accent, null),
    };
}

public sealed record AccentColors(Color Start, Color End)
{
    /// <summary>Gradiente diagonal para tarjetas protagonistas.</summary>
    public LinearGradientBrush Gradient => new(
        [new GradientStop(Start, 0f), new GradientStop(End, 1f)],
        new Point(0, 0),
        new Point(1, 1));

    /// <summary>Versión translúcida para fondos de insignias y chips.</summary>
    public Color Soft => End.WithAlpha(0.22f);
}
