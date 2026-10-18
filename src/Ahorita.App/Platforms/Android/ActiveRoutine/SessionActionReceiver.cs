using System.Globalization;
using Ahorita.App.Droid.Notifications;
using Ahorita.App.Resources.Strings;
using Ahorita.Application.Sessions;
using Android.App;
using Android.Content;
using AndroidX.Core.App;

namespace Ahorita.App.Droid.ActiveRoutine;

/// <summary>Botones "Listo" y "Saltar" de la notificación: avanzar sin abrir la app.</summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class SessionActionReceiver : BroadcastReceiver
{
    public static PendingIntent Intent(Context context, Guid sessionId, SessionAction action)
    {
        var intent = new Intent(context, typeof(SessionActionReceiver))
            .SetAction("com.fsoftt.ahorita.SESSION_ACTION." + action)
            .PutExtra(AhoritaNotifications.ExtraSessionId, sessionId.ToString())
            .PutExtra(AhoritaNotifications.ExtraAction, action.ToString());

        return PendingIntent.GetBroadcast(context, AhoritaNotifications.IdFor(sessionId, (int)action), intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null
            || intent is null
            || !Guid.TryParse(intent.GetStringExtra(AhoritaNotifications.ExtraSessionId), out var sessionId)
            || !Enum.TryParse<SessionAction>(intent.GetStringExtra(AhoritaNotifications.ExtraAction), out var action))
        {
            return;
        }

        ServiceAccess.RunInBackground(this, async () =>
        {
            if (ServiceAccess.UseCases is not { } useCases)
                return;

            // El caso de uso actualiza o quita la notificación persistente por su cuenta.
            var result = await useCases.ExecuteAsync((ApplySessionAction useCase) => useCase.ExecuteAsync(sessionId, action)).ConfigureAwait(false);
            if (result.IsSuccess && result.Value.IsFinished)
                ShowFinished(context, result.Value);
        });
    }

    private static void ShowFinished(Context context, SessionSnapshot session)
    {
        var builder = new NotificationCompat.Builder(context, AhoritaNotifications.CelebrationsChannel);
        builder.SetSmallIcon(Resource.Drawable.ic_notification);
        builder.SetContentTitle(AppResources.Notif_FinishedTitle);
        builder.SetContentText(string.Format(CultureInfo.CurrentCulture, AppResources.Notif_FinishedText, session.RoutineName));
        builder.SetAutoCancel(true);
        builder.SetTimeoutAfter((long)TimeSpan.FromHours(1).TotalMilliseconds);
        builder.SetContentIntent(AhoritaNotifications.OpenAppIntent(context));

        AhoritaNotifications.Notify(context, AhoritaNotifications.CelebrationNotificationId, builder.Build()!);
    }
}
