namespace Ahorita.Application.Abstractions;

/// <summary>
/// Ejecuta un caso de uso dentro de su propio scope de DI, para que cada operación
/// tenga una unidad de trabajo (y un DbContext) nueva. En MAUI no hay "request"
/// que delimite el scope, así que lo hacemos explícito.
/// </summary>
public interface IUseCaseExecutor
{
    Task<TResult> ExecuteAsync<TUseCase, TResult>(Func<TUseCase, Task<TResult>> execute)
        where TUseCase : notnull;
}
