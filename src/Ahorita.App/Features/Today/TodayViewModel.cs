using System.Globalization;
using Ahorita.App.Common;
using Ahorita.App.Features.Shared;
using Ahorita.App.Resources.Strings;
using Ahorita.App.Services;
using Ahorita.Application.Abstractions;
using Ahorita.Application.Routines;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Ahorita.App.Features.Today;

public sealed partial class TodayViewModel : ViewModelBase
{
    private readonly IUseCaseExecutor _useCases;
    private readonly TimeProvider _time;

    public TodayViewModel(IUseCaseExecutor useCases, TimeProvider time, IToastService toast, ILogger<TodayViewModel> logger)
        : base(toast, logger)
    {
        _useCases = useCases;
        _time = time;
        Greeting = string.Empty;
        DateText = string.Empty;
        FeaturedLabel = string.Empty;
        Upcoming = [];
        AnyTime = [];
        Templates = [];
    }

    [ObservableProperty]
    public partial string Greeting { get; set; }

    [ObservableProperty]
    public partial string DateText { get; set; }

    [ObservableProperty]
    public partial RoutineCardModel? Featured { get; set; }

    [ObservableProperty]
    public partial bool HasFeatured { get; set; }

    [ObservableProperty]
    public partial string FeaturedLabel { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<RoutineCardModel> Upcoming { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<RoutineCardModel> AnyTime { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<TemplateCardModel> Templates { get; set; }

    [ObservableProperty]
    public partial bool IsFirstRun { get; set; }

    [ObservableProperty]
    public partial bool IsFreeDay { get; set; }

    public override Task OnAppearingAsync() => RunSafelyAsync(LoadAsync);

    [RelayCommand]
    private Task StartRoutineAsync(RoutineCardModel? routine) =>
        // El modo Ahora (Fase 2) se conectará aquí.
        routine is null ? Task.CompletedTask : Toast.ShowAsync(AppResources.Focus_ComingSoon);

    [RelayCommand]
    private Task AddTemplateAsync(TemplateCardModel template) => RunSafelyAsync(async () =>
    {
        if (await TemplateAdding.AddAsync(_useCases, Toast, template))
            await LoadAsync();
    });

    private async Task LoadAsync()
    {
        UpdateHeader(_time.GetLocalNow());

        var overview = await _useCases.ExecuteAsync((GetTodayOverview useCase) => useCase.ExecuteAsync());

        Featured = overview.Featured is { } featured ? RoutineCardModel.From(featured) : null;
        HasFeatured = Featured is not null;
        FeaturedLabel = overview.Featured is { IsOnDemand: false } ? AppResources.Today_NextUp : AppResources.Today_AnyTime;
        Upcoming = overview.Upcoming.Select(RoutineCardModel.From).ToArray();
        AnyTime = overview.AnyTime.Select(RoutineCardModel.From).ToArray();
        IsFirstRun = overview.IsFirstRun;
        IsFreeDay = !overview.IsFirstRun && overview.Featured is null;

        if (IsFirstRun && Templates.Count == 0)
        {
            var templates = await _useCases.ExecuteAsync((ListRoutineTemplates useCase) => useCase.ExecuteAsync());
            Templates = templates.Select(TemplateCardModel.From).ToArray();
        }
    }

    private void UpdateHeader(DateTimeOffset now)
    {
        var greeting = now.Hour switch
        {
            < 12 => AppResources.Greeting_Morning + " ☀️",
            < 19 => AppResources.Greeting_Afternoon + " 🌤️",
            _ => AppResources.Greeting_Evening + " 🌙",
        };

        var culture = CultureInfo.CurrentCulture;
        var date = now.ToString(AppResources.Today_DateFormat, culture);

        Greeting = greeting;
        DateText = date.Length > 0 ? char.ToUpper(date[0], culture) + date[1..] : date;
    }
}
