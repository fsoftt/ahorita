using CommunityToolkit.Mvvm.ComponentModel;

namespace Ahorita.App.Features.Editor;

/// <summary>Opción de un grupo de chips de minutos (duración de paso, aviso previo…).</summary>
public sealed partial class MinutesOption(int? minutes, string label) : ObservableObject
{
    public int? Minutes { get; } = minutes;

    public string Label { get; } = label;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public static IReadOnlyList<MinutesOption> Create(IEnumerable<int?> values, Func<int?, string> label, int? selected)
    {
        var options = values.Select(v => new MinutesOption(v, label(v))).ToArray();
        Select(options, selected);
        return options;
    }

    public static void Select(IEnumerable<MinutesOption> options, int? minutes)
    {
        foreach (var option in options)
            option.IsSelected = option.Minutes == minutes;
    }
}
