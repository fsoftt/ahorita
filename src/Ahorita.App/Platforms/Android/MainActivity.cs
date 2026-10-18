using Ahorita.App.Droid.Notifications;
using Ahorita.App.Services;
using Ahorita.App.Theming;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;

namespace Ahorita.App;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Al volver desde "Recientes" (o tras recrear la actividad) Android reentrega el
        // intent original: abrirlo otra vez reiniciaría una rutina que quizá ya terminaste.
        var fromHistory = Intent is not null && Intent.Flags.HasFlag(ActivityFlags.LaunchedFromHistory);
        if (savedInstanceState is null && !fromHistory)
            HandleDeepLink(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        HandleDeepLink(intent);
    }

    protected override void OnResume()
    {
        base.OnResume();

        // "Quitar animaciones" de Android pone la escala de animación en 0.
        var scale = Settings.Global.GetFloat(ContentResolver, Settings.Global.AnimatorDurationScale, 1f);
        Motion.SystemReduced = scale == 0f;
    }

    /// <summary>"Empezar" desde una notificación abre directo el modo Ahora.</summary>
    private static void HandleDeepLink(Intent? intent)
    {
        if (intent?.GetStringExtra(AhoritaNotifications.ExtraRoutineId) is not { } raw || !Guid.TryParse(raw, out var routineId))
            return;

        intent.RemoveExtra(AhoritaNotifications.ExtraRoutineId);
        IPlatformApplication.Current?.Services.GetService<DeepLinks>()?.OpenRoutine(routineId);
    }
}
