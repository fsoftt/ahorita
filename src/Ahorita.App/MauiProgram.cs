using Ahorita.App.Features.Celebration;
using Ahorita.App.Features.Editor;
using Ahorita.App.Features.Focus;
using Ahorita.App.Features.Progress;
using Ahorita.App.Features.Routines;
using Ahorita.App.Features.Today;
using Ahorita.App.Services;
using Ahorita.App.Theming;
using Ahorita.Application;
using Ahorita.Application.Abstractions;
using Ahorita.Infrastructure;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Ahorita.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Lexend-Regular.ttf", "LexendRegular");
                fonts.AddFont("Lexend-Medium.ttf", "LexendMedium");
                fonts.AddFont("Lexend-SemiBold.ttf", "LexendSemiBold");
                fonts.AddFont("Outfit-Bold.ttf", "OutfitBold");
                fonts.AddFont("Outfit-ExtraBold.ttf", "OutfitExtraBold");
            });

        builder.Services
            .AddApplication()
            .AddInfrastructure(Path.Combine(FileSystem.AppDataDirectory, "ahorita.db"))
            .AddPresentation();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        PlatformTweaks.Apply();

        return builder.Build();
    }

    private static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<IToastService, ToastService>();
        services.AddSingleton<INavigationService, ShellNavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<AppShell>();
        services.AddSingleton<DeepLinks>();
        services.AddSingleton<AppStartup>();
        services.AddSingleton<PermissionPrompts>();
#if ANDROID
        services.AddSingleton<IReminderScheduler, Droid.Reminders.AndroidReminderScheduler>();
        services.AddSingleton<IActiveRoutineNotifier, Droid.ActiveRoutine.AndroidActiveRoutineNotifier>();
        services.AddSingleton<IDevicePermissions, Droid.AndroidDevicePermissions>();
#endif
        services.AddSingleton(Preferences.Default);
        services.AddSingleton<PreferencesAppSettings>();
        services.AddSingleton<IAppSettings>(sp => sp.GetRequiredService<PreferencesAppSettings>());

        services.AddTransient<TodayPage>().AddTransient<TodayViewModel>();
        services.AddTransient<RoutinesPage>().AddTransient<RoutinesViewModel>();
        services.AddTransient<ProgressPage>().AddTransient<ProgressViewModel>();
        services.AddTransient<RoutineEditorPage>().AddTransient<RoutineEditorViewModel>();
        services.AddTransient<StepEditorPage>().AddTransient<StepEditorViewModel>();
        services.AddTransient<FocusPage>().AddTransient<FocusViewModel>();
        services.AddTransient<CelebrationPage>().AddTransient<CelebrationViewModel>();

        return services;
    }
}
