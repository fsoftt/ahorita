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
// La voz se cancela y libera en OnDisappearing. No se implementa IDisposable a propósito:
// el contenedor de DI retendría el ViewModel (transitorio) hasta cerrar la app.
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001", Justification = "Se libera en OnDisappearing.")]
public sealed partial class FocusViewModel : ViewModelBase, IQueryAttributable
{
    public const string RoutineIdParameter = "routineId";

    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(250);

    /// <summary>Cada cuántos ticks se relee la sesión (la notificación también puede avanzarla).</summary>
    private const int TicksPerSync = 12;

    private readonly IUseCaseExecutor _useCases;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialogs;
    private readonly TimeProvider _time;
    private readonly IAppSettings _settings;
    private readonly PermissionPrompts _permissions;
    private IDispatcherTimer? _timer;
    private SessionSnapshot? _session;
    private Guid _routineId;
    private bool _timeUpNotified;
    private int _ticks;
    private bool _syncing;
    private bool _acting;
    private CancellationTokenSource? _speech;

    public FocusViewModel(
        IUseCaseExecutor useCases,
        INavigationService navigation,
        IDialogService dialogs,
        TimeProvider time,
        IAppSettings settings,
        PermissionPrompts permissions,
        IToastService toast,
        ILogger<FocusViewModel> logger)
        : base(toast, logger)
    {
        _permissions = permissions;
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
        SpeakCurrentStep();

        // La notificación persistente se publicó al empezar; si el permiso llega ahora, se vuelve a mostrar.
        if (await _permissions.EnsureActiveRoutineAsync())
            await _useCases.ExecuteAsync((RestoreSessionNotification useCase) => useCase.ExecuteAsync());
    });

    public override void OnDisappearing()
    {
        _speech?.Cancel();
        _speech?.Dispose();
        _speech = null;
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
        if (_session is null || _acting)
            return;

        _acting = true;
        try
        {
            await ActCoreAsync(_session.Id, action);
        }
        finally
        {
            _acting = false;
        }
    });

    private async Task ActCoreAsync(Guid sessionId, SessionAction action)
    {
        var result = await _useCases.ExecuteAsync((ApplySessionAction useCase) => useCase.ExecuteAsync(sessionId, action));
        if (result.IsFailure)
        {
            await Toast.ShowAsync(ErrorText.For(result.Error));
            return;
        }

        var snapshot = result.Value;
        if (snapshot.IsFinished)
        {
            await CelebrateAsync(snapshot);
            return;
        }

        var stepChanged = action is SessionAction.CompleteStep or SessionAction.SkipStep or SessionAction.PostponeStep;
        Apply(snapshot, animate: stepChanged);
        if (stepChanged)
        {
            if (action == SessionAction.CompleteStep)
                Haptics.Confirm();

            SpeakCurrentStep();
        }
    }

    private Task CelebrateAsync(SessionSnapshot snapshot)
    {
        StopTicking();
        return _navigation.GoToAsync("../" + Routes.Celebration, new Dictionary<string, object>
        {
            [CelebrationViewModel.SessionIdParameter] = snapshot.Id,
        });
    }

    /// <summary>Relee la sesión por si se avanzó desde la notificación.</summary>
    private async Task SyncAsync()
    {
        if (_session is null || _syncing || _acting)
            return;

        _syncing = true;
        try
        {
            var result = await _useCases.ExecuteAsync((GetSession useCase) => useCase.ExecuteAsync(_session.Id));
            if (result.IsFailure || _acting)
                return;

            if (result.Value.IsFinished)
                await CelebrateAsync(result.Value);
            else if (result.Value.Position != _session.Position || result.Value.Status != _session.Status)
                Apply(result.Value, animate: true);
        }
#pragma warning disable CA1031 // Sincronización de fondo: si falla, se intenta en el siguiente ciclo.
        catch (Exception)
#pragma warning restore CA1031
        {
        }
        finally
        {
            _syncing = false;
        }
    }

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
        _timer.Tick += (_, _) =>
        {
            Tick();
            if (++_ticks % TicksPerSync == 0)
                _ = SyncAsync();
        };
        _timer.Start();
    }

    private void StopTicking()
    {
        _timer?.Stop();
        _timer = null;
    }

    /// <summary>
    /// Lee el paso sin esperar a que termine: los botones siguen respondiendo mientras habla,
    /// y una frase nueva interrumpe la anterior.
    /// </summary>
    private void SpeakCurrentStep()
    {
        _speech?.Cancel();
        _speech?.Dispose();
        _speech = null;

        if (!_settings.ReadStepsAloud || string.IsNullOrWhiteSpace(StepTitle))
            return;

        _speech = new CancellationTokenSource();
        _ = SpeakAsync(StepTitle, _speech.Token);
    }

    private static async Task SpeakAsync(string text, CancellationToken cancellationToken)
    {
        try
        {
            await TextToSpeech.Default.SpeakAsync(text, cancelToken: cancellationToken);
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
