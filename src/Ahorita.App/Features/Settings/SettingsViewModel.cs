using System.Globalization;
using Ahorita.App.Common;
using Ahorita.App.Resources.Strings;
using Ahorita.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Ahorita.App.Features.Settings;

public sealed partial class SettingsViewModel : ViewModelBase
{
    private readonly IAppSettings _settings;
    private readonly IDevicePermissions _permissions;
    private readonly INavigationService _navigation;

    public SettingsViewModel(
        IAppSettings settings,
        IDevicePermissions permissions,
        INavigationService navigation,
        IToastService toast,
        ILogger<SettingsViewModel> logger)
        : base(toast, logger)
    {
        _settings = settings;
        _permissions = permissions;
        _navigation = navigation;

        LowStimulus = settings.LowStimulus;
        Celebrations = settings.Celebrations;
        HapticsEnabled = settings.HapticsEnabled;
        ReadStepsAloud = settings.ReadStepsAloud;
        Themes =
        [
            new ThemeOption(ThemePreference.System, AppResources.Settings_ThemeSystem),
            new ThemeOption(ThemePreference.Light, AppResources.Settings_ThemeLight),
            new ThemeOption(ThemePreference.Dark, AppResources.Settings_ThemeDark),
        ];
        SelectTheme(settings.Theme);
        NotificationsStatus = string.Empty;
        ExactAlarmsStatus = string.Empty;
        Version = string.Format(CultureInfo.CurrentCulture, AppResources.Settings_Version, AppInfo.Current.VersionString);
    }

    public IReadOnlyList<ThemeOption> Themes { get; }

    public string Version { get; }

    [ObservableProperty]
    public partial bool LowStimulus { get; set; }

    [ObservableProperty]
    public partial bool Celebrations { get; set; }

    [ObservableProperty]
    public partial bool HapticsEnabled { get; set; }

    [ObservableProperty]
    public partial bool ReadStepsAloud { get; set; }

    [ObservableProperty]
    public partial bool HasNotifications { get; set; }

    [ObservableProperty]
    public partial string NotificationsStatus { get; set; }

    [ObservableProperty]
    public partial bool HasExactAlarms { get; set; }

    [ObservableProperty]
    public partial string ExactAlarmsStatus { get; set; }

    public override async Task OnAppearingAsync()
    {
        HasNotifications = await _permissions.HasNotificationsAsync();
        HasExactAlarms = _permissions.HasExactAlarms;
        NotificationsStatus = HasNotifications ? AppResources.Settings_Enabled : AppResources.Settings_Disabled;
        ExactAlarmsStatus = HasExactAlarms ? AppResources.Settings_Enabled : AppResources.Settings_Disabled;
    }

    partial void OnLowStimulusChanged(bool value) => _settings.LowStimulus = value;

    partial void OnCelebrationsChanged(bool value) => _settings.Celebrations = value;

    partial void OnHapticsEnabledChanged(bool value) => _settings.HapticsEnabled = value;

    partial void OnReadStepsAloudChanged(bool value) => _settings.ReadStepsAloud = value;

    [RelayCommand]
    private void PickTheme(ThemeOption option)
    {
        _settings.Theme = option.Theme;
        SelectTheme(option.Theme);
    }

    [RelayCommand]
    private async Task EnableNotificationsAsync()
    {
        // Si Android ya no muestra el diálogo (se rechazó antes), abrimos los ajustes de la app.
        if (!await _permissions.RequestNotificationsAsync())
            AppInfo.Current.ShowSettingsUI();

        await OnAppearingAsync();
    }

    [RelayCommand]
    private void EnableExactAlarms() => _permissions.OpenExactAlarmSettings();

    [RelayCommand]
    private Task CloseAsync() => _navigation.GoBackAsync();

    private void SelectTheme(ThemePreference theme)
    {
        foreach (var option in Themes)
            option.IsSelected = option.Theme == theme;
    }
}

public sealed partial class ThemeOption(ThemePreference theme, string label) : ObservableObject
{
    public ThemePreference Theme { get; } = theme;

    public string Label { get; } = label;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
