using Ahorita.Application.Sessions;

namespace Ahorita.Application.Abstractions;

/// <summary>
/// Refleja la rutina en curso fuera de la app (en Android, una notificación persistente
/// con el paso actual, cuenta regresiva y botones). Fallar aquí nunca rompe la sesión.
/// </summary>
public interface IActiveRoutineNotifier
{
    Task ShowAsync(SessionSnapshot session, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}
