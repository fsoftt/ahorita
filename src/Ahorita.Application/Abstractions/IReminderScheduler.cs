using Ahorita.Application.Reminders;

namespace Ahorita.Application.Abstractions;

/// <summary>Programa avisos del sistema (en Android, alarmas + notificaciones).</summary>
public interface IReminderScheduler
{
    /// <summary>Reemplaza todos los recordatorios programados por <paramref name="reminders"/>.</summary>
    Task ReplaceAllAsync(IReadOnlyList<ScheduledReminder> reminders, CancellationToken cancellationToken = default);
}
