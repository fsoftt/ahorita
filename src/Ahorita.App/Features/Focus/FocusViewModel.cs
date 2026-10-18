using System.Globalization;
using Ahorita.App.Common;
using Ahorita.App.Features.Celebration;
using Ahorita.App.Resources.Strings;
using Ahorita.App.Services;
using Ahorita.App.Theming;
using Ahorita.Application.Abstractions;
using Ahorita.Application.Sessions;
using Ahorita.Domain.Routines;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Ahorita.App.Features.Focus;

/// <summary>
/// Modo Ahora: un paso a la vez. El estado vive en el dominio; aquí solo se
/// proyecta el tiempo cada fracción de segundo a partir de la última foto.
/// </summary>
public sealed partial class FocusViewModel : ViewModelBase, IQueryAttributable
{
    public const string RoutineIdParameter = "routineId";

    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(250);

    private readonly IUseCaseExecutor _useCases;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialogs;
    private readonly TimeProvider _time;
    private readonly IAppSettings _settings;
    private IDispatcherTimer? _timer;
    private SessionSnapshot? _session;
    private Guid _routineId;
    private bool _timeUpNotified;

    public FocusViewModel(
        IUseCaseExecutor useCases,
        INavigationService navigation,
        IDialogService dialogs,
        TimeProvider time,
        IAppSettings settings,
        IToastService toast,
        ILogger<FocusViewModel> logger)
        : base(toast, logger)
    {
        _useCases = useCases;
        _navigation = navigation;
        _dialogs = dialogs;
        _time = time;
        _settings = settings;
        RoutineName = string.Empty;
        StepEmoji = string.Empty;
        StepTitle = string.Empty;
        PositionText = string.Empty;
        NextText = string.Empty;
        TimeText = string.Empty;
        EstimatedEndText = string.Empty;
        RemainingFraction = 1;
    }

    /// <summary>Se dispara al cambiar de paso, para animar la entrada del siguiente.</summary>
    public event EventHandler? StepChanged;

    [ObservableProperty]
    public partial string RoutineName { get; set; }

    [ObservableProperty]
    public partial RoutineAccent Accent { get; set; }

    [ObservableProperty]
    public partial string StepEmoji { get; set; }

    [ObservableProperty]
    public partial string StepTitle { get; set; }

    [ObservableProperty]
    public partial bool IsOptionalStep { get; set; }

    [ObservableProperty]
    public partial string PositionText { get; set; }

    [ObservableProperty]
    public partial string NextText { get; set; }

    [ObservableProperty]
    public partial string TimeText { get; set; }

    [ObservableProperty]
    public partial bool HasTimer { get; set; }

    [ObservableProperty]
    public partial bool IsOvertime { get; set; }

    [ObservableProperty]
    public partial double RemainingFraction { get; set; }

    [ObservableProperty]
    public partial string EstimatedEndText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PauseLabel))]
    public partial bool IsPaused { get; set; }

    [ObservableProperty]
    public partial bool CanPostpone { get; set; }

    [ObservableProperty]
    public partial bool IsReady { get; set; }

    public string PauseLabel => IsPaused ? AppResources.Focus_Resume : AppResources.Focus_Pause;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(RoutineIdParameter, out var id) && id is Guid routineId)
            _routineId = routineId;
    }

    public override Task OnAppearingAsync() => RunSafelyAsync(async () =>
    {
        KeepScreenOn(true);

        // Empezar es idempotente: si la rutina ya está en curso, se retoma donde iba.
        var result = await _useCases.ExecuteAsync((StartSession useCase) => useCase.ExecuteAsync(_routineId));
        if (result.IsFailure)
        {
            await Toast.ShowAsync(ErrorText.For(result.Error));
            await _navigation.GoBackAsync();
            return;
        }

        Apply(result.Value, animate: false);
        IsReady = true;
        StartTicking();
        await SpeakCurrentStepAsync();
    });

    public override void OnDisappearing()
    {
        StopTicking();
        KeepScreenOn(false);
    }

    [RelayCommand]
    private Task DoneAsync() => ActAsync(SessionAction.CompleteStep);

    [RelayCommand]
    private Task SkipAsync() => ActAsync(SessionAction.SkipStep);

    [RelayCommand]
    private async Task PostponeAsync()
    {
        await ActAsync(SessionAction.PostponeStep);
        await Toast.ShowAsync(AppResources.Focus_Postponed);
    }

    [RelayCommand]
    private Task TogglePauseAsync() => ActAsync(IsPaused ? SessionAction.Resume : SessionAction.Pause);

    [RelayCommand]
    private async Task EndAsync()
    {
        var confirmed = await _dialogs.ConfirmAsync(
            AppResources.Focus_EndTitle, AppResources.Focus_EndMessage, AppResources.Focus_EndConfirm, AppResources.Focus_KeepGoing);
        if (confirmed)
            await ActAsync(SessionAction.EndEarly);
    }

    /// <summary>Salir de la pantalla no detiene la rutina: sigue en la notificación.</summary>
    [RelayCommand]
    private Task CloseAsync() => _navigation.GoBackAsync();

    private Task ActAsync(SessionAction action) => RunSafelyAsync(async () =>
    {
        if (_session is null)
            return;

        var result = await _useCases.ExecuteAsync((ApplySessionAction useCase) => useCase.ExecuteAsync(_session.Id, action));
        if (result.IsFailure)
        {
            await Toast.ShowAsync(ErrorText.For(result.Error));
            return;
        }

        var snapshot = result.Value;
        if (snapshot.IsFinished)
        {
            StopTicking();
            await _navigation.GoToAsync("../" + Routes.Celebration, new Dictionary<string, object>
            {
                [CelebrationViewModel.SessionIdParameter] = snapshot.Id,
            });
            return;
        }

        var stepChanged = action is SessionAction.CompleteStep or SessionAction.SkipStep or SessionAction.PostponeStep;
        Apply(snapshot, animate: stepChanged);
        if (stepChanged)
        {
            if (action == SessionAction.CompleteStep)
                Haptics.Confirm();

            await SpeakCurrentStepAsync();
        }
    });

    private void Apply(SessionSnapshot snapshot, bool animate)
    {
        var stepChanged = _session?.CurrentStep != snapshot.CurrentStep || _session?.Position != snapshot.Position;
        _session = snapshot;
        if (stepChanged)
            _timeUpNotified = false;

        var culture = CultureInfo.CurrentCulture;
        RoutineName = snapshot.RoutineName;
        Accent = snapshot.Accent;
        StepEmoji = snapshot.CurrentStep?.Emoji ?? snapshot.RoutineEmoji;
        StepTitle = snapshot.CurrentStep?.Title ?? string.Empty;
        IsOptionalStep = snapshot.CurrentStep?.IsOptional ?? false;
        PositionText = string.Format(culture, AppResources.Focus_Position, snapshot.Position, snapshot.TotalSteps);
        NextText = snapshot.NextStep is { } next
            ? string.Format(culture, AppResources.Focus_Next, $"{next.Emoji} {next.Title}".Trim())
            : AppResources.Focus_LastStep;
        HasTimer = snapshot.CurrentStep?.PlannedDuration is not null;
        IsPaused = snapshot.IsPaused;
        CanPostpone = snapshot.CanPostpone;
        Tick();

        if (animate && stepChanged)
            StepChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Tick()
    {
        if (_session is not { CurrentStep: { } step } session)
            return;

        var now = _time.GetUtcNow();
        if (step.PlannedDuration is { } planned)
        {
            var remaining = session.RemainingAt(now)!.Value;
            IsOvertime = remaining < TimeSpan.Zero;
            RemainingFraction = IsOvertime ? 0 : remaining.TotalSeconds / planned.TotalSeconds;
            TimeText = IsOvertime ? "+" + Format(remaining.Negate()) : Format(remaining);

            if (IsOvertime && !_timeUpNotified && !session.IsPaused)
            {
                _timeUpNotified = true;
                Haptics.Confirm();
                _ = Toast.ShowAsync(AppResources.Focus_TimeUp);
            }
        }
        else
        {
            IsOvertime = false;
            RemainingFraction = 1;
            TimeText = AppResources.Focus_NoTimer;
        }

        var end = TimeZoneInfo.ConvertTime(session.EstimatedEndAt(now), _time.LocalTimeZone);
        EstimatedEndText = IsPaused
            ? AppResources.Focus_Paused
            : string.Format(CultureInfo.CurrentCulture, AppResources.Focus_EndsAt, end.ToString("t", CultureInfo.CurrentCulture));
    }

    private void StartTicking()
    {
        StopTicking();
        var dispatcher = Dispatcher.GetForCurrentThread();
        if (dispatcher is null)
            return;

        _timer = dispatcher.CreateTimer();
        _timer.Interval = TickInterval;
        _timer.Tick += (_, _) => Tick();
        _timer.Start();
    }

    private void StopTicking()
    {
        _timer?.Stop();
        _timer = null;
    }

    private async Task SpeakCurrentStepAsync()
    {
        if (!_settings.ReadStepsAloud || string.IsNullOrWhiteSpace(StepTitle))
            return;

        try
        {
            await TextToSpeech.Default.SpeakAsync(StepTitle);
        }
#pragma warning disable CA1031 // Leer en voz alta es un extra: si falla, seguimos en silencio.
        catch (Exception)
#pragma warning restore CA1031
        {
        }
    }

    private static void KeepScreenOn(bool keepOn)
    {
        try
        {
            DeviceDisplay.Current.KeepScreenOn = keepOn;
        }
        catch (FeatureNotSupportedException)
        {
        }
    }

    private static string Format(TimeSpan span) =>
        span.TotalHours >= 1
            ? span.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
            : span.ToString(@"m\:ss", CultureInfo.InvariantCulture);
}
