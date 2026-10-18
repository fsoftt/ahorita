using System.Globalization;
using Ahorita.App.Resources.Strings;
using Ahorita.App.Services;
using Ahorita.App.Theming;
using Ahorita.Application.Abstractions;
using Ahorita.Application.Routines;

namespace Ahorita.App.Features.Shared;

/// <summary>Acción compartida "agregar rutina desde plantilla", con su celebración breve.</summary>
internal static class TemplateAdding
{
    public static async Task<bool> AddAsync(IUseCaseExecutor useCases, IToastService toast, PermissionPrompts permissions, TemplateCardModel template)
    {
        var result = await useCases.ExecuteAsync((CreateRoutineFromTemplate useCase) => useCase.ExecuteAsync(template.Key));

        if (result.IsFailure)
        {
            await toast.ShowAsync(AppResources.Error_Generic);
            return false;
        }

        Haptics.Confirm();
        await toast.ShowAsync(string.Format(CultureInfo.CurrentCulture, AppResources.Routine_Created, result.Value.Name));
        if (!result.Value.IsOnDemand)
            await permissions.EnsureRemindersAsync();

        return true;
    }
}
