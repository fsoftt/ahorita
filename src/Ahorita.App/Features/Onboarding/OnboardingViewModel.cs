using Ahorita.App.Common;
using Ahorita.App.Features.Shared;
using Ahorita.App.Services;
using Ahorita.App.Theming;
using Ahorita.Application.Abstractions;
using Ahorita.Application.Routines;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Ahorita.App.Features.Onboarding;

/// <summary>Bienvenida en 3 pantallas, todas saltables. Se muestra una sola vez.</summary>
public sealed partial class OnboardingViewModel : ViewModelBase
{
    private const int LastStep = 2;

    private readonly IUseCaseExecutor _useCases;
    private readonly INavigationService _navigation;
    private readonly IDevicePermissions _permissions;
    private readonly IAppSettings _settings;

    public OnboardingViewModel(
        IUseCaseExecutor useCases,
        INavigationService navigation,
        IDevicePermissions permissions,
        IAppSettings settings,
        IToastService toast,
        ILogger<OnboardingViewModel> logger)
        : base(toast, logger)
    {
        _useCases = useCases;
        _navigation = navigation;
        _permissions = permissions;
        _settings = settings;
        Templates = [];
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWelcome), nameof(IsTemplates), nameof(IsReminders), nameof(IsLastStep))]
    public partial int Step { get; set; }

    public bool IsWelcome => Step == 0;
    public bool IsTemplates => Step == 1;
    public bool IsReminders => Step == 2;
    public bool IsLastStep => Step == LastStep;

    [ObservableProperty]
    public partial IReadOnlyList<SelectableTemplate> Templates { get; set; }

    public override Task OnAppearingAsync()
    {
        // Se marca al mostrarse: si la persona sale con "atrás", no volvemos a insistir.
        _settings.OnboardingCompleted = true;

        return Templates.Count > 0
            ? Task.CompletedTask
            : RunSafelyAsync(async () =>
            {
                var templates = await _useCases.ExecuteAsync((ListRoutineTemplates useCase) => useCase.ExecuteAsync());
                Templates = templates.Select(t => new SelectableTemplate(TemplateCardModel.From(t))).ToArray();
                if (Templates.Count > 0)
                    Templates[0].IsSelected = true;
            });
    }

    [RelayCommand]
    private void Next()
    {
        Haptics.Tap();
        if (Step < LastStep)
            Step++;
    }

    [RelayCommand]
    private static void ToggleTemplate(SelectableTemplate template)
    {
        Haptics.Tap();
        template.IsSelected = !template.IsSelected;
    }

    [RelayCommand]
    private Task EnableRemindersAsync() => FinishAsync(requestNotifications: true);

    [RelayCommand]
    private Task FinishAsync() => FinishAsync(requestNotifications: false);

    [RelayCommand]
    private Task SkipAsync() => _navigation.GoToAsync(Routes.Today);

    private Task FinishAsync(bool requestNotifications) => RunSafelyAsync(async () =>
    {
        if (requestNotifications)
            await _permissions.RequestNotificationsAsync();

        foreach (var template in Templates.Where(t => t.IsSelected))
            await _useCases.ExecuteAsync((CreateRoutineFromTemplate useCase) => useCase.ExecuteAsync(template.Template.Key));

        Haptics.Confirm();
        await _navigation.GoToAsync(Routes.Today);
    });
}

public sealed partial class SelectableTemplate(TemplateCardModel template) : ObservableObject
{
    public TemplateCardModel Template { get; } = template;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
