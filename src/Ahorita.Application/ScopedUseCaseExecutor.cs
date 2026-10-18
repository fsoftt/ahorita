using Ahorita.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Ahorita.Application;

internal sealed class ScopedUseCaseExecutor(IServiceScopeFactory scopeFactory, IStorageInitializer storage) : IUseCaseExecutor
{
    public async Task<TResult> ExecuteAsync<TUseCase, TResult>(Func<TUseCase, Task<TResult>> execute)
        where TUseCase : notnull
    {
        await storage.EnsureInitializedAsync().ConfigureAwait(false);

        await using var scope = scopeFactory.CreateAsyncScope();
        var useCase = scope.ServiceProvider.GetRequiredService<TUseCase>();
        return await execute(useCase).ConfigureAwait(false);
    }
}
