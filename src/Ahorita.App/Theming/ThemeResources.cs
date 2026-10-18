namespace Ahorita.App.Theming;

/// <summary>Acceso tipado a los tokens de App.xaml desde código.</summary>
public static class ThemeResources
{
    public static Color Color(string key) => Get<Color>(key);

    public static T Get<T>(string key) =>
        Microsoft.Maui.Controls.Application.Current!.Resources.TryGetValue(key, out var value) && value is T typed
            ? typed
            : throw new InvalidOperationException($"Falta el recurso '{key}'.");

    /// <summary>Aplica un par de tokens claro/oscuro que reacciona al cambio de tema.</summary>
    public static void SetThemeColor(BindableObject target, BindableProperty property, string lightKey, string darkKey) =>
        target.SetAppThemeColor(property, Color(lightKey), Color(darkKey));

    public static void SetThemeBrush(BindableObject target, BindableProperty property, string lightKey, string darkKey) =>
        target.SetAppTheme<Brush>(property, new SolidColorBrush(Color(lightKey)), new SolidColorBrush(Color(darkKey)));
}
