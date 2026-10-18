using Ahorita.App.Common;
using Ahorita.App.Features.Shared;
using Ahorita.App.Resources.Strings;
using Ahorita.App.Services;
using Ahorita.App.Theming;
using Ahorita.Domain.Routines;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Ahorita.App.Features.Editor;

/// <summary>Edita un paso sobre valores temporales; solo al guardar se copian al paso real.</summary>
public sealed partial class StepEditorViewModel : ViewModelBase, IQueryAttributable
{
    public const string RequestParameter = "request";

    private static readonly int?[] DurationValues = [null, 1, 2, 3, 5, 10, 15, 20, 25, 30, 45, 60];

    private readonly INavigationService _navigation;
    private StepEditRequest? _request;

    public StepEditorViewModel(INavigationService navigation, IToastService toast, ILogger<StepEditorViewModel> logger)
        : base(toast, logger)
    {
        _navigation = navigation;
        Title = string.Empty;
        Durations = MinutesOption.Create(DurationValues, DurationLabel, selected: 5);
    }

    public IReadOnlyList<string> EmojiOptions { get; } = EmojiCatalog.Steps;

    [ObservableProperty]
    public partial IReadOnlyList<MinutesOption> Durations { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string? Emoji { get; set; }

    [ObservableProperty]
    public partial bool IsOptional { get; set; }

    [ObservableProperty]
    public partial bool IsExisting { get; set; }

    public string PageTitle => IsExisting ? AppResources.StepEditor_EditTitle : AppResources.StepEditor_NewTitle;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue(RequestParameter, out var value) || value is not StepEditRequest request)
            return;

        _request = request;
        IsExisting = !request.IsNew;
        Title = request.Step.Title;
        Emoji = request.Step.Emoji;
        IsOptional = request.Step.IsOptional;
        if (request.Step.Minutes is { } minutes && !DurationValues.Contains(minutes))
        {
            var values = DurationValues.Append(minutes).OrderBy(v => v ?? -1).ToArray();
            Durations = MinutesOption.Create(values, DurationLabel, minutes);
        }
        else
        {
            MinutesOption.Select(Durations, request.Step.Minutes);
        }
        OnPropertyChanged(nameof(PageTitle));
    }

    [RelayCommand]
    private void PickEmoji(string emoji)
    {
        Haptics.Tap();
        Emoji = Emoji == emoji ? null : emoji;
    }

    [RelayCommand]
    private void PickDuration(MinutesOption option) => MinutesOption.Select(Durations, option.Minutes);

    [RelayCommand]
    private Task CancelAsync() => _navigation.GoBackAsync();

    [RelayCommand]
    private Task SaveAsync()
    {
        if (_request is null)
            return _navigation.GoBackAsync();

        if (string.IsNullOrWhiteSpace(Title))
            return Toast.ShowAsync(AppResources.Error_Routine_StepTitleRequired);

        if (Title.Trim().Length > RoutineStep.MaxTitleLength)
            return Toast.ShowAsync(AppResources.Error_Routine_StepTitleTooLong);

        var step = _request.Step;
        step.Title = Title.Trim();
        step.Emoji = Emoji;
        step.Minutes = Durations.FirstOrDefault(d => d.IsSelected)?.Minutes;
        step.IsOptional = IsOptional;

        if (_request.IsNew)
            _request.Owner.Add(step);

        Haptics.Tap();
        return _navigation.GoBackAsync();
    }

    [RelayCommand]
    private Task MoveUpAsync() => MoveAsync(-1);

    [RelayCommand]
    private Task MoveDownAsync() => MoveAsync(+1);

    [RelayCommand]
    private Task DeleteAsync()
    {
        if (_request is { IsNew: false })
            _request.Owner.Remove(_request.Step);

        return _navigation.GoBackAsync();
    }

    /// <summary>Alternativa accesible a arrastrar y soltar.</summary>
    private Task MoveAsync(int delta)
    {
        if (_request is not { IsNew: false } request)
            return Task.CompletedTask;

        var index = request.Owner.IndexOf(request.Step);
        var target = index + delta;
        if (index < 0 || target < 0 || target >= request.Owner.Count)
            return Task.CompletedTask;

        request.Owner.Move(index, target);
        Haptics.Tap();
        return Toast.ShowAsync(AppResources.StepEditor_Moved);
    }

    private static string DurationLabel(int? minutes) =>
        minutes is { } m ? RoutineText.Minutes(m) : AppResources.Step_NoTimer;
}
