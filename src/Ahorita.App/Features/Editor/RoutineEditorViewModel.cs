using System.Collections.ObjectModel;
using Ahorita.App.Common;
using Ahorita.App.Features.Shared;
using Ahorita.App.Resources.Strings;
using Ahorita.App.Services;
using Ahorita.App.Theming;
using Ahorita.Application.Abstractions;
using Ahorita.Application.Routines;
using Ahorita.Domain.Routines;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Ahorita.App.Features.Editor;

/// <summary>
/// Editor de rutina. Trabaja sobre una copia local y guarda todo de una vez,
/// así cancelar nunca deja cambios a medias.
/// </summary>
public sealed partial class RoutineEditorViewModel : ViewModelBase, IQueryAttributable
{
    public const string RoutineIdParameter = "routineId";

    private static readonly DayOfWeek[] MondayFirst =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday,
    ];

    private readonly IUseCaseExecutor _useCases;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialogs;
    private readonly PermissionPrompts _permissions;
    private Guid? _routineId;
    private bool _loaded;

    public RoutineEditorViewModel(
        IUseCaseExecutor useCases,
        INavigationService navigation,
        IDialogService dialogs,
        PermissionPrompts permissions,
        MissingPermissionsModel missingPermissions,
        IToastService toast,
        ILogger<RoutineEditorViewModel> logger)
        : base(toast, logger)
    {
        _useCases = useCases;
        _navigation = navigation;
        _dialogs = dialogs;
        _permissions = permissions;
        Permissions = missingPermissions;

        Name = string.Empty;
        Emoji = EmojiCatalog.Routines[^1];
        Accent = RoutineAccent.Tangerine;
        StartTime = new TimeSpan(7, 0, 0);
        Accents = Enum.GetValues<RoutineAccent>().Select(a => new AccentOption(a)).ToArray();
        Days = MondayFirst.Select(d => new DayOption(d)).ToArray();
        LeadTimes = MinutesOption.Create([0, 5, 10, 15, 30], LeadTimeLabel, (int)RoutineSchedule.DefaultReminderLeadTime.TotalMinutes);
        Steps = [];
        Steps.CollectionChanged += (_, _) => OnPropertyChanged(nameof(StepsSummary));
        SelectAccent(Accent);
    }

    /// <summary>Aviso de permisos faltantes mientras el borrador tenga días.</summary>
    public MissingPermissionsModel Permissions { get; }

    public IReadOnlyList<string> EmojiOptions { get; } = EmojiCatalog.Routines;

    public IReadOnlyList<AccentOption> Accents { get; }

    public IReadOnlyList<DayOption> Days { get; }

    public IReadOnlyList<MinutesOption> LeadTimes { get; }

    public ObservableCollection<StepItem> Steps { get; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string Emoji { get; set; }

    [ObservableProperty]
    public partial RoutineAccent Accent { get; set; }

    [ObservableProperty]
    public partial bool IsScheduled { get; set; }

    [ObservableProperty]
    public partial TimeSpan StartTime { get; set; }

    [ObservableProperty]
    public partial bool IsExisting { get; set; }

    public string Title => IsExisting ? AppResources.Editor_EditTitle : AppResources.Editor_NewTitle;

    public string PreviewName => string.IsNullOrWhiteSpace(Name) ? AppResources.Editor_NamePlaceholder : Name;

    public string StepsSummary => Steps.Count == 0
        ? AppResources.Editor_NoStepsYet
        : RoutineText.Meta(Steps.Count, TimeSpan.FromMinutes(Steps.Sum(s => s.Minutes ?? 0)));

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // Shell vuelve a llamar con un diccionario vacío al regresar del editor de paso:
        // solo una llamada con id cambia la rutina que se edita.
        if (!query.TryGetValue(RoutineIdParameter, out var id) || id is not Guid routineId)
            return;

        _routineId = routineId;
        IsExisting = true;
        OnPropertyChanged(nameof(Title));
    }

    public override async Task OnAppearingAsync()
    {
        await LoadRoutineAsync();
        await RefreshPermissionsAsync();
    }

    public override Task OnResumedAsync() => RefreshPermissionsAsync();

    private Task LoadRoutineAsync()
    {
        // Al volver del editor de paso no recargamos: perderíamos los cambios locales.
        if (_loaded || _routineId is not { } id)
            return Task.CompletedTask;

        return RunSafelyAsync(async () =>
        {
            var result = await _useCases.ExecuteAsync((GetRoutineForEditing useCase) => useCase.ExecuteAsync(id));
            if (result.IsFailure)
            {
                await Toast.ShowAsync(ErrorText.For(result.Error));
                await _navigation.GoBackAsync();
                return;
            }

            Apply(result.Value);
            _loaded = true;
        });
    }

    partial void OnNameChanged(string value) => OnPropertyChanged(nameof(PreviewName));

    partial void OnAccentChanged(RoutineAccent value) => SelectAccent(value);

    [RelayCommand]
    private void PickEmoji(string emoji)
    {
        Haptics.Tap();
        Emoji = emoji;
    }

    [RelayCommand]
    private void PickAccent(AccentOption option)
    {
        Haptics.Tap();
        Accent = option.Accent;
    }

    [RelayCommand]
    private void SetScheduled(bool scheduled)
    {
        IsScheduled = scheduled;
        if (scheduled && Days.All(d => !d.IsSelected))
        {
            foreach (var day in Days.Where(d => WeekDays.Weekdays.Includes(d.Day)))
                day.IsSelected = true;
        }

        _ = RefreshPermissionsAsync();
    }

    [RelayCommand]
    private void ToggleDay(DayOption day)
    {
        day.IsSelected = !day.IsSelected;
        _ = RefreshPermissionsAsync();
    }

    [RelayCommand]
    private void PickLeadTime(MinutesOption option) => MinutesOption.Select(LeadTimes, option.Minutes);

    [RelayCommand]
    private Task AddStepAsync() => OpenStepEditorAsync(new StepItem(null, string.Empty, null, 5, isOptional: false), isNew: true);

    [RelayCommand]
    private Task EditStepAsync(StepItem step) => OpenStepEditorAsync(step, isNew: false);

    [RelayCommand]
    private Task CancelAsync() => _navigation.GoBackAsync();

    [RelayCommand]
    private Task SaveAsync() => RunSafelyAsync(async () =>
    {
        var draft = ToDraft();
        var result = await _useCases.ExecuteAsync((SaveRoutine useCase) => useCase.ExecuteAsync(draft));
        if (result.IsFailure)
        {
            await Toast.ShowAsync(ErrorText.For(result.Error));
            return;
        }

        Haptics.Confirm();
        if (draft.Days != WeekDays.None)
            await _permissions.EnsureRemindersAsync();

        await _navigation.GoBackAsync();
    });

    [RelayCommand]
    private Task DeleteAsync() => RunSafelyAsync(async () =>
    {
        if (_routineId is not { } id)
            return;

        var confirmed = await _dialogs.ConfirmAsync(
            AppResources.Editor_DeleteTitle, AppResources.Editor_DeleteMessage, AppResources.Editor_DeleteConfirm, AppResources.Common_Cancel);
        if (!confirmed)
            return;

        var result = await _useCases.ExecuteAsync((ArchiveRoutine useCase) => useCase.ExecuteAsync(id));
        if (result.IsFailure)
        {
            await Toast.ShowAsync(ErrorText.For(result.Error));
            return;
        }

        await _navigation.GoBackAsync();
    });

    private Task OpenStepEditorAsync(StepItem step, bool isNew) =>
        _navigation.GoToAsync(Routes.StepEditor, new Dictionary<string, object>
        {
            [StepEditorViewModel.RequestParameter] = new StepEditRequest(step, Steps, isNew),
        });

    private void Apply(RoutineDraft draft)
    {
        Name = draft.Name;
        Emoji = draft.Emoji;
        Accent = draft.Accent;
        IsScheduled = draft.Days != WeekDays.None;
        StartTime = draft.StartTime?.ToTimeSpan() ?? StartTime;
        foreach (var day in Days)
            day.IsSelected = draft.Days.Includes(day.Day);

        if (draft.ReminderLeadTime is { } lead)
            MinutesOption.Select(LeadTimes, (int)lead.TotalMinutes);

        Steps.Clear();
        foreach (var step in draft.Steps)
            Steps.Add(StepItem.From(step));
    }

    private RoutineDraft ToDraft()
    {
        var days = IsScheduled
            ? Days.Where(d => d.IsSelected).Aggregate(WeekDays.None, (all, d) => all | d.Flag)
            : WeekDays.None;
        var leadMinutes = LeadTimes.FirstOrDefault(o => o.IsSelected)?.Minutes ?? 0;

        return new RoutineDraft(
            _routineId,
            Name,
            Emoji,
            Accent,
            days,
            days == WeekDays.None ? null : TimeOnly.FromTimeSpan(StartTime),
            days == WeekDays.None ? null : TimeSpan.FromMinutes(leadMinutes),
            Steps.Select(s => s.ToDraft()).ToArray());
    }

    // El aviso aparece en cuanto el borrador tiene días, antes de guardar.
    private Task RefreshPermissionsAsync() =>
        RunSafelyAsync(() => Permissions.RefreshAsync(IsScheduled && Days.Any(d => d.IsSelected)));

    private void SelectAccent(RoutineAccent accent)
    {
        foreach (var option in Accents)
            option.IsSelected = option.Accent == accent;
    }

    private static string LeadTimeLabel(int? minutes) =>
        minutes is null or 0 ? AppResources.Editor_NoHeadsUp : RoutineText.Minutes(minutes.Value);
}
