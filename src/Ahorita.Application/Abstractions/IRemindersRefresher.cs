namespace Ahorita.Application.Abstractions;

/// <summary>
/// Vuelve a calcular y programar los recordatorios tras cambiar las rutinas.
/// Fallar al programar nunca debe impedir guardar: la implementación registra y sigue.
/// </summary>
public interface IRemindersRefresher
{
    /// <param name="notBefore">
    /// Ignora ocurrencias anteriores a este instante. Al dispararse una alarma se pasa su hora,
    /// así una alarma que llega unos milisegundos antes no se vuelve a programar.
    /// </param>
    Task RefreshAsync(DateTimeOffset? notBefore = null, CancellationToken cancellationToken = default);
}
