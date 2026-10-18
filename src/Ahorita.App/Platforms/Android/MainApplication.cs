using Ahorita.App.Droid.Notifications;
using Android.App;
using Android.Runtime;

namespace Ahorita.App;

[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
{
    public override void OnCreate()
    {
        base.OnCreate();
        AhoritaNotifications.EnsureChannels(this);
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
