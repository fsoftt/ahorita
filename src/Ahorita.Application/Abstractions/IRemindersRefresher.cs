namespace Ahorita.Application.Abstractions;

/// <summary>
/// Vuelve a calcular y programar los recordatorios tras cambiar las rutinas.
/// Fallar al programar nunca debe impedir guardar: la implementación registra y sigue.
/// </summary>
public interface IRemindersRefresher
{
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
