using Ahorita.App.Features.Shared;
using Ahorita.App.Resources.Strings;
using Ahorita.Application.Routines;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ahorita.App.Features.Editor;

/// <summary>Paso editable dentro del editor de rutina.</summary>
public sealed partial class StepItem : ObservableObject
{
    public StepItem(Guid? id, string title, string? emoji, int? minutes, bool isOptional)
    {
        Id = id;
        Title = title;
        Emoji = emoji;
        Minutes = minutes;
        IsOptional = isOptional;
    }

    public Guid? Id { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayEmoji))]
    public partial string? Emoji { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Details))]
    public partial int? Minutes { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Details))]
    public partial bool IsOptional { get; set; }

    public string DisplayEmoji => string.IsNullOrWhiteSpace(Emoji) ? "•" : Emoji;

    public string Details
    {
        get
        {
            var time = Minutes is { } m ? RoutineText.Minutes(m) : AppResources.Step_NoTimer;
            return IsOptional ? time + AppResources.Routine_Separator + AppResources.Step_Optional : time;
        }
    }

    public static StepItem From(StepDraft draft) => new(
        draft.Id,
        draft.Title,
        draft.Emoji,
        draft.Duration is { } d ? (int)Math.Ceiling(d.TotalMinutes) : null,
        draft.IsOptional);

    public StepDraft ToDraft() => new(Id, Title, Emoji, Minutes is { } m ? TimeSpan.FromMinutes(m) : null, IsOptional);
}
