using Ahorita.App.Features.Focus;

namespace Ahorita.App.Services;

/// <summary>
/// Enlaces que llegan desde fuera (p. ej. "Empezar" en una notificación). Si la UI
/// todavía no está lista, se guardan y se abren en cuanto Shell aparece.
/// </summary>
public sealed class DeepLinks(INavigationService navigation)
{
    private readonly Lock _gate = new();
    private Guid? _pendingRoutine;
    private bool _shellReady;

    public void OpenRoutine(Guid routineId)
    {
        lock (_gate)
        {
            if (!_shellReady)
            {
                _pendingRoutine = routineId;
                return;
            }
        }

        _ = NavigateAsync(routineId);
    }

    public void ShellReady()
    {
        Guid? pending;
        lock (_gate)
        {
            _shellReady = true;
            pending = _pendingRoutine;
            _pendingRoutine = null;
        }

        if (pending is { } routineId)
            _ = NavigateAsync(routineId);
    }

    // Ruta absoluta: deja "Hoy" debajo para que "atrás" tenga sentido.
    private Task NavigateAsync(Guid routineId) =>
        navigation.GoToAsync($"{Routes.Today}/{Routes.Focus}", new Dictionary<string, object>
        {
            [FocusViewModel.RoutineIdParameter] = routineId,
        });
}
