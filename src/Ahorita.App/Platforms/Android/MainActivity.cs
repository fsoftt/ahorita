using Ahorita.App.Theming;
using Android.App;
using Android.Content.PM;
using Android.Provider;

namespace Ahorita.App;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnResume()
    {
        base.OnResume();

        // "Quitar animaciones" de Android pone la escala de animación en 0.
        var scale = Settings.Global.GetFloat(ContentResolver, Settings.Global.AnimatorDurationScale, 1f);
        Motion.SystemReduced = scale == 0f;
    }
}
