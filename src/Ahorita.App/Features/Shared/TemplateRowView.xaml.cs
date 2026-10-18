using System.Windows.Input;

namespace Ahorita.App.Features.Shared;

public partial class TemplateRowView : ContentView
{
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(TemplateRowView));

    public TemplateRowView() => InitializeComponent();

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
}
