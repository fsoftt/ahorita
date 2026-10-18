using Ahorita.Domain.Routines;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ahorita.App.Features.Editor;

public sealed partial class AccentOption(RoutineAccent accent) : ObservableObject
{
    public RoutineAccent Accent { get; } = accent;

    public string Name => Accent.ToString();

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
