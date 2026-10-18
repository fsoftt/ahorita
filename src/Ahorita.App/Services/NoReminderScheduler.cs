using Ahorita.Application.Abstractions;
using Ahorita.Application.Reminders;

namespace Ahorita.App.Services;

/// <summary>Temporal hasta conectar el programador de alarmas de Android (Fase 3).</summary>
internal sealed class NoReminderScheduler : IReminderScheduler
{
    public Task ReplaceAllAsync(IReadOnlyList<ScheduledReminder> reminders, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
