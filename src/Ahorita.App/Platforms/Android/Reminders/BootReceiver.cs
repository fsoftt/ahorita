using Ahorita.Application.Reminders;
using Ahorita.Application.Sessions;
using Android.App;
using Android.Content;

namespace Ahorita.App.Droid.Reminders;

/// <summary>
/// Android borra las alarmas al reiniciar o actualizar la app, y la hora puede cambiar:
/// en todos esos casos (y al conceder alarmas exactas) se vuelve a planificar desde la base de datos.
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter([
    Intent.ActionBootCompleted,
    Intent.ActionMyPackageReplaced,
    Intent.ActionTimeChanged,
    Intent.ActionTimezoneChanged,
    "android.app.action.SCHEDULE_EXACT_ALARM_PERMISSION_STATE_CHANGED",
])]
public sealed class BootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent) =>
        ServiceAccess.RunInBackground(this, async () =>
        {
            if (ServiceAccess.UseCases is not { } useCases)
                return;

            await useCases.ExecuteAsync((RefreshReminders useCase) => useCase.ExecuteAsync()).ConfigureAwait(false);
            await useCases.ExecuteAsync((RestoreSessionNotification useCase) => useCase.ExecuteAsync()).ConfigureAwait(false);
        });
}
