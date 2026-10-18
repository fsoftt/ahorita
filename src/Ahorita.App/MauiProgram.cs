using Ahorita.App.Features.Celebration;
using Ahorita.App.Features.Donate;
using Ahorita.App.Features.Editor;
using Ahorita.App.Features.Focus;
using Ahorita.App.Features.Onboarding;
using Ahorita.App.Features.Progress;
using Ahorita.App.Features.Routines;
using Ahorita.App.Features.Settings;
using Ahorita.App.Features.Today;
using Ahorita.App.Services;
using Ahorita.App.Theming;
using Ahorita.Application;
using Ahorita.Application.Abstractions;
using Ahorita.Infrastructure;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.AdMob;
using Plugin.AdMob.Configuration;

namespace Ahorita.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Antes que nada: la UI, las plantillas y las notificaciones usan este idioma.
        Localization.Apply(Localization.Read(Preferences.Default));

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseAdMob(
                androidDefaultBannerAdUnitId: AdMobIds.BannerOrTest,
                androidDefaultRewardedAdUnitId: AdMobIds.RewardedOrTest)
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

        // Sin ids propios configurados, solo anuncios de prueba de Google.
        AdConfig.UseTestAdUnitIds = !AdMobIds.IsConfigured;

        return builder.Build();
    }

    private static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<IToastService, ToastService>();
        services.AddSingleton<INavigationService, ShellNavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddTransient<AppShell>();
        services.AddSingleton<DeepLinks>();
        services.AddSingleton<AppStartup>();
        services.AddSingleton<PermissionPrompts>();
        services.AddSingleton<UiRestarter>();
        services.AddSingleton<SupportAdsPreference>();
        services.AddSingleton<IDonateAdService, AdMobDonateAdService>();
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
        services.AddTransient<SettingsPage>().AddTransient<SettingsViewModel>();
        services.AddTransient<OnboardingPage>().AddTransient<OnboardingViewModel>();
        services.AddTransient<DonatePage>().AddTransient<DonateViewModel>();

        return services;
    }
}
