using Ahorita.App.Services;
using Ahorita.Application.Reminders;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Ahorita.App.Features.Shared;

/// <summary>Estado del aviso "faltan permisos" que comparten la lista de rutinas y el editor.</summary>
public sealed partial class MissingPermissionsModel(ReminderPermissionsMonitor monitor) : ObservableObject
{
    [ObservableProperty]
    public partial bool IsVisible { get; set; }

    [ObservableProperty]
    public partial bool NotificationsMissing { get; set; }

    [ObservableProperty]
    public partial bool ExactAlarmsMissing { get; set; }

    /// <param name="needed">Hay alguna rutina que avisa: si no, no hay nada que advertir.</param>
    public async Task RefreshAsync(bool needed)
    {
        var missing = (await monitor.CheckAsync()).WhenNeeded(needed);
        NotificationsMissing = missing.Notifications;
        ExactAlarmsMissing = missing.ExactAlarms;
        IsVisible = missing.Any;
    }

    [RelayCommand]
    private void OpenNotificationSettings() => monitor.OpenNotificationSettings();

    [RelayCommand]
    private void OpenExactAlarmSettings() => monitor.OpenExactAlarmSettings();
}
