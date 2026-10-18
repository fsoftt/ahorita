using System.Globalization;
using Ahorita.App.Common;
using Ahorita.App.Features.Focus;
using Ahorita.App.Features.Shared;
using Ahorita.App.Resources.Strings;
using Ahorita.App.Services;
using Ahorita.Application.Abstractions;
using Ahorita.Application.Routines;
using Ahorita.Application.Sessions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Ahorita.App.Features.Today;

public sealed partial class TodayViewModel : ViewModelBase
{
    private readonly IUseCaseExecutor _useCases;
    private readonly PermissionPrompts _permissions;
    private readonly INavigationService _navigation;
    private readonly TimeProvider _time;

    public TodayViewModel(
        IUseCaseExecutor useCases,
        PermissionPrompts permissions,
        INavigationService navigation,
        TimeProvider time,
        IToastService toast,
        ILogger<TodayViewModel> logger)
        : base(toast, logger)
    {
        _useCases = useCases;
        _permissions = permissions;
        _navigation = navigation;
        _time = time;
        HeroActionText = AppResources.Today_StartNow;
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
    public partial string HeroActionText { get; set; }

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
        routine is null
            ? Task.CompletedTask
            : _navigation.GoToAsync(Routes.Focus, new Dictionary<string, object>
            {
                [FocusViewModel.RoutineIdParameter] = routine.Id,
            });

    [RelayCommand]
    private Task OpenSettingsAsync() => _navigation.GoToAsync(Routes.Settings);

    [RelayCommand]
    private Task AddTemplateAsync(TemplateCardModel template) => RunSafelyAsync(async () =>
    {
        if (await TemplateAdding.AddAsync(_useCases, Toast, _permissions, template))
            await LoadAsync();
    });

    private async Task LoadAsync()
    {
        UpdateHeader(_time.GetLocalNow());

        var overview = await _useCases.ExecuteAsync((GetTodayOverview useCase) => useCase.ExecuteAsync());
        var active = await _useCases.ExecuteAsync((GetActiveSession useCase) => useCase.ExecuteAsync());

        if (active is not null)
        {
            // Una rutina en curso siempre es la protagonista: retomarla es un toque.
            Featured = new RoutineCardModel(
                active.RoutineId, active.RoutineName, active.RoutineEmoji, active.Accent,
                active.CurrentStep is { } step ? $"{step.Emoji} {step.Title}".Trim() : string.Empty,
                AppResources.Today_Continue,
                CanStart: true);
            FeaturedLabel = string.Format(CultureInfo.CurrentCulture, AppResources.Today_InProgress, active.Position, active.TotalSteps);
            HeroActionText = AppResources.Today_Continue;
        }
        else
        {
            Featured = overview.Featured is { } featured ? RoutineCardModel.From(featured) : null;
            FeaturedLabel = overview.Featured is { IsOnDemand: false } ? AppResources.Today_NextUp : AppResources.Today_AnyTime;
            HeroActionText = AppResources.Today_StartNow;
        }

        HasFeatured = Featured is not null;
        var featuredId = Featured?.Id;
        Upcoming = overview.Upcoming.Where(r => r.Id != featuredId).Select(RoutineCardModel.From).ToArray();
        AnyTime = overview.AnyTime.Where(r => r.Id != featuredId).Select(RoutineCardModel.From).ToArray();
        IsFirstRun = overview.IsFirstRun;
        IsFreeDay = !overview.IsFirstRun && Featured is null;

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
