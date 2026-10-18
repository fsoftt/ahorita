using Ahorita.App.Common;
using Ahorita.App.Features.Shared;
using Ahorita.App.Resources.Strings;
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

    public RoutinesViewModel(IUseCaseExecutor useCases, IToastService toast, ILogger<RoutinesViewModel> logger)
        : base(toast, logger)
    {
        _useCases = useCases;
        Routines = [];
        Templates = [];
    }

    [ObservableProperty]
    public partial IReadOnlyList<RoutineRowModel> Routines { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<TemplateCardModel> Templates { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

    public override Task OnAppearingAsync() => RunSafelyAsync(LoadAsync);

    [RelayCommand]
    private Task OpenRoutineAsync(RoutineRowModel routine) =>
        // El editor de rutinas (Fase 1) se conectará aquí.
        Toast.ShowAsync(AppResources.Routines_EditorComingSoon);

    [RelayCommand]
    private Task AddTemplateAsync(TemplateCardModel template) => RunSafelyAsync(async () =>
    {
        if (await TemplateAdding.AddAsync(_useCases, Toast, template))
            await LoadAsync();
    });

    private async Task LoadAsync()
    {
        var routines = await _useCases.ExecuteAsync((ListRoutines useCase) => useCase.ExecuteAsync());
        Routines = routines.Select(RoutineRowModel.From).ToArray();
        IsEmpty = Routines.Count == 0;

        if (Templates.Count == 0)
        {
            var templates = await _useCases.ExecuteAsync((ListRoutineTemplates useCase) => useCase.ExecuteAsync());
            Templates = templates.Select(TemplateCardModel.From).ToArray();
        }
    }
}
