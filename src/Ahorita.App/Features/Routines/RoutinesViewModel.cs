using Ahorita.App.Common;
using Ahorita.App.Features.Editor;
using Ahorita.App.Features.Shared;
using Ahorita.App.Services;
using Ahorita.Application.Abstractions;
using Ahorita.Application.Routines;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Ahorita.App.Features.Routines;

public sealed partial class RoutinesViewModel : ViewModelBase
{
    private readonly IUseCaseExecutor _useCases;
    private readonly PermissionPrompts _permissions;
    private readonly INavigationService _navigation;

    public RoutinesViewModel(IUseCaseExecutor useCases, PermissionPrompts permissions, MissingPermissionsModel missingPermissions, INavigationService navigation, IToastService toast, ILogger<RoutinesViewModel> logger)
        : base(toast, logger)
    {
        _useCases = useCases;
        _permissions = permissions;
        Permissions = missingPermissions;
        _navigation = navigation;
        Routines = [];
        Templates = [];
    }

    [ObservableProperty]
    public partial IReadOnlyList<RoutineRowModel> Routines { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<TemplateCardModel> Templates { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

    /// <summary>Aviso de permisos faltantes: solo si alguna rutina avisa.</summary>
    public MissingPermissionsModel Permissions { get; }

    public override Task OnAppearingAsync() => RunSafelyAsync(LoadAsync);

    public override Task OnResumedAsync() => RunSafelyAsync(RefreshPermissionsAsync);

    [RelayCommand]
    private Task OpenRoutineAsync(RoutineRowModel routine) =>
        _navigation.GoToAsync(Routes.RoutineEditor, new Dictionary<string, object>
        {
            [RoutineEditorViewModel.RoutineIdParameter] = routine.Id,
        });

    [RelayCommand]
    private Task NewRoutineAsync() => _navigation.GoToAsync(Routes.RoutineEditor);

    [RelayCommand]
    private Task AddTemplateAsync(TemplateCardModel template) => RunSafelyAsync(async () =>
    {
        if (await TemplateAdding.AddAsync(_useCases, Toast, _permissions, template))
            await LoadAsync();
    });

    private async Task RefreshPermissionsAsync()
    {
        var routines = await _useCases.ExecuteAsync((ListRoutines useCase) => useCase.ExecuteAsync());
        await Permissions.RefreshAsync(routines.Any(r => r.HasReminders));
    }

    private async Task LoadAsync()
    {
        var routines = await _useCases.ExecuteAsync((ListRoutines useCase) => useCase.ExecuteAsync());
        Routines = routines.Select(RoutineRowModel.From).ToArray();
        IsEmpty = Routines.Count == 0;
        await Permissions.RefreshAsync(routines.Any(r => r.HasReminders));

        if (Templates.Count == 0)
        {
            var templates = await _useCases.ExecuteAsync((ListRoutineTemplates useCase) => useCase.ExecuteAsync());
            Templates = templates.Select(TemplateCardModel.From).ToArray();
        }
    }
}
