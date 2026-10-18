using System.Globalization;
using Ahorita.App.Common;
using Ahorita.App.Resources.Strings;
using Ahorita.App.Services;
using Ahorita.Application.Abstractions;
using Ahorita.Application.Sessions;
using Ahorita.Domain.Routines;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Ahorita.App.Features.Celebration;

/// <summary>Celebración al terminar: siempre positiva, también con progreso parcial.</summary>
public sealed partial class CelebrationViewModel : ViewModelBase, IQueryAttributable
{
    public const string SessionIdParameter = "sessionId";

    private static readonly string[] Messages =
    [
        AppResources.Celebrate_Msg1, AppResources.Celebrate_Msg2, AppResources.Celebrate_Msg3,
        AppResources.Celebrate_Msg4, AppResources.Celebrate_Msg5, AppResources.Celebrate_Msg6,
    ];

    private readonly IUseCaseExecutor _useCases;
    private readonly INavigationService _navigation;
    private readonly IAppSettings _settings;
    private Guid _sessionId;

    public CelebrationViewModel(
        IUseCaseExecutor useCases,
        INavigationService navigation,
        IAppSettings settings,
        IToastService toast,
        ILogger<CelebrationViewModel> logger)
        : base(toast, logger)
    {
        _useCases = useCases;
        _navigation = navigation;
        _settings = settings;
        Emoji = "🎉";
        Title = string.Empty;
        Subtitle = string.Empty;
        StepsText = string.Empty;
        DurationText = string.Empty;
        Message = Messages[Random.Shared.Next(Messages.Length)];
    }

    /// <summary>La página lanza el confeti cuando los datos están listos.</summary>
    public event EventHandler? Ready;

    [ObservableProperty]
    public partial string Emoji { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string Subtitle { get; set; }

    [ObservableProperty]
    public partial string StepsText { get; set; }

    [ObservableProperty]
    public partial string DurationText { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; }

    [ObservableProperty]
    public partial RoutineAccent Accent { get; set; }

    public bool ShowsEffects => _settings.Celebrations;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(SessionIdParameter, out var id) && id is Guid sessionId)
            _sessionId = sessionId;
    }

    public override Task OnAppearingAsync() => RunSafelyAsync(async () =>
    {
        var result = await _useCases.ExecuteAsync((GetSession useCase) => useCase.ExecuteAsync(_sessionId));
        if (result.IsFailure)
            return;

        var session = result.Value;
        var culture = CultureInfo.CurrentCulture;
        var complete = session.CompletionRatio >= 1;

        Accent = session.Accent;
        Emoji = complete ? "🎉" : session.RoutineEmoji;
        Title = complete ? AppResources.Celebrate_TitleComplete : AppResources.Celebrate_TitlePartial;
        Subtitle = complete
            ? string.Format(culture, AppResources.Celebrate_SubtitleComplete, session.RoutineName)
            : string.Format(culture, AppResources.Celebrate_SubtitlePartial, session.DoneSteps, session.TotalSteps);
        StepsText = string.Format(culture, AppResources.Celebrate_Steps, session.DoneSteps);
        DurationText = string.Format(culture, AppResources.Celebrate_Duration, Math.Max(1, (int)Math.Round(session.TotalElapsed.TotalMinutes)));

        Ready?.Invoke(this, EventArgs.Empty);
    });

    [RelayCommand]
    private Task BackHomeAsync() => _navigation.GoToAsync(Routes.Today);
}
