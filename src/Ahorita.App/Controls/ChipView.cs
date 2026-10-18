using System.Windows.Input;
using Ahorita.App.Resources.Strings;
using Ahorita.App.Theming;

namespace Ahorita.App.Controls;

/// <summary>Chip seleccionable en forma de píldora (días, duraciones, opciones).</summary>
public sealed class ChipView : Border
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(ChipView), string.Empty,
        propertyChanged: (b, _, _) => ((ChipView)b).ApplyState());

    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected), typeof(bool), typeof(ChipView), false,
        propertyChanged: (b, _, _) => ((ChipView)b).ApplyState());

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ChipView));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ChipView));

    private readonly Label _label = new()
    {
        HorizontalTextAlignment = TextAlignment.Center,
        VerticalTextAlignment = TextAlignment.Center,
        LineBreakMode = LineBreakMode.NoWrap,
    };

    public ChipView()
    {
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 999 };
        StrokeThickness = 1.5;
        Padding = new Thickness(16, 8);
        MinimumHeightRequest = 44;
        MinimumWidthRequest = 44;
        Content = _label;
        _label.FontFamily = ThemeResources.Get<string>("FontBodySemiBold");
        _label.FontSize = 15;

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            if (Command?.CanExecute(CommandParameter) != true)
                return;

            Haptics.Tap();
            Command.Execute(CommandParameter);
        };
        GestureRecognizers.Add(tap);
        ApplyState();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    private void ApplyState()
    {
        _label.Text = Text;
        SemanticProperties.SetDescription(this, IsSelected ? $"{Text}, {AppResources.A11y_Selected}" : Text);

        if (IsSelected)
        {
            ThemeResources.SetThemeColor(this, BackgroundColorProperty, "BrandLight", "BrandDark");
            ThemeResources.SetThemeBrush(this, StrokeProperty, "BrandLight", "BrandDark");
            ThemeResources.SetThemeColor(_label, Label.TextColorProperty, "OnBrand", "OnAccent");
        }
        else
        {
            ThemeResources.SetThemeColor(this, BackgroundColorProperty, "SurfaceLight", "SurfaceDark");
            ThemeResources.SetThemeBrush(this, StrokeProperty, "OutlineLight", "OutlineDark");
            ThemeResources.SetThemeColor(_label, Label.TextColorProperty, "InkLight", "InkDark");
        }
    }
}
