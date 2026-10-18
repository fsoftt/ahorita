using Ahorita.Application.Abstractions;
using Ahorita.Application.Sessions;

namespace Ahorita.App.Services;

/// <summary>Temporal hasta conectar la notificación persistente de Android (Fase 3).</summary>
internal sealed class NoActiveRoutineNotifier : IActiveRoutineNotifier
{
    public Task ShowAsync(SessionSnapshot session, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task ClearAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
