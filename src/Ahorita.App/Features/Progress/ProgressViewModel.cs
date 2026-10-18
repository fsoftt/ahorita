using System.Globalization;
using Ahorita.App.Common;
using Ahorita.App.Resources.Strings;
using Ahorita.App.Services;
using Ahorita.App.Theming;
using Ahorita.Application.Abstractions;
using Ahorita.Application.Progress;
using Ahorita.Application.Routines;
using Ahorita.Domain.Progress;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Ahorita.App.Features.Progress;

/// <summary>Historial amable: constancia flexible, mapa sin rojo y sugerencias accionables.</summary>
public sealed partial class ProgressViewModel : ViewModelBase
{
    private readonly IUseCaseExecutor _useCases;

    public ProgressViewModel(IUseCaseExecutor useCases, IToastService toast, ILogger<ProgressViewModel> logger)
        : base(toast, logger)
    {
        _useCases = useCases;
        ConsistencyText = string.Empty;
        WeekDays = [];
        Map = [];
        Insights = [];
    }

    [ObservableProperty]
    public partial bool HasActivity { get; set; }

    [ObservableProperty]
    public partial string ConsistencyText { get; set; }

    [ObservableProperty]
    public partial double ConsistencyRatio { get; set; }

    [ObservableProperty]
    public partial int RoutinesCompleted { get; set; }

    [ObservableProperty]
    public partial int StepsDone { get; set; }

    [ObservableProperty]
    public partial int MinutesInRoutines { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<DayCell> WeekDays { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<DayCell> Map { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<InsightItem> Insights { get; set; }

    public override Task OnAppearingAsync() => RunSafelyAsync(LoadAsync);

    [RelayCommand]
    private Task AdjustAsync(InsightItem insight) => RunSafelyAsync(async () =>
    {
        var result = await _useCases.ExecuteAsync((AdjustStepDuration useCase) =>
            useCase.ExecuteAsync(insight.Insight.RoutineId, insight.Insight.StepId, insight.Insight.SuggestedDuration));

        await Toast.ShowAsync(result.IsSuccess ? AppResources.Progress_Adjusted : ErrorText.For(result.Error));
        if (result.IsSuccess)
        {
            Haptics.Confirm();
            Insights = Insights.Where(i => i != insight).ToArray();
        }
    });

    private async Task LoadAsync()
    {
        var summary = await _useCases.ExecuteAsync((GetProgressSummary useCase) => useCase.ExecuteAsync());
        var culture = CultureInfo.CurrentCulture;

        HasActivity = summary.HasAnyActivity;
        ConsistencyText = string.Format(culture, AppResources.Progress_Consistency, summary.Consistency.ActiveDays);
        ConsistencyRatio = summary.Consistency.Ratio;
        RoutinesCompleted = summary.RoutinesCompleted;
        StepsDone = summary.StepsDone;
        MinutesInRoutines = summary.MinutesInRoutines;
        Map = summary.Days.Select(DayCell.From).ToArray();
        WeekDays = Map.TakeLast(7).ToArray();
        Insights = summary.Insights.Select(InsightItem.From).ToArray();
    }
}

public sealed record DayCell(string Label, int Level, string Description)
{
    public bool IsActive => Level > 0;

    public static DayCell From(DayActivity day)
    {
        var culture = CultureInfo.CurrentCulture;
        var date = day.Day.ToDateTime(TimeOnly.MinValue);
        var label = culture.DateTimeFormat.GetShortestDayName(date.DayOfWeek).ToUpper(culture);
        return new DayCell(label, day.Level, $"{date.ToString("d", culture)}: {day.StepsDone}");
    }
}

public sealed record InsightItem(DurationInsight Insight, string Text, string ActionText)
{
    public static InsightItem From(DurationInsight insight)
    {
        var culture = CultureInfo.CurrentCulture;
        var name = $"{insight.StepEmoji} {insight.StepTitle}".Trim();
        var typical = (int)Math.Round(insight.TypicalActual.TotalMinutes);
        var planned = (int)Math.Round(insight.Planned.TotalMinutes);
        return new InsightItem(
            insight,
            string.Format(culture, AppResources.Progress_Insight, name, typical, planned),
            string.Format(culture, AppResources.Progress_Adjust, (int)insight.SuggestedDuration.TotalMinutes));
    }
}
