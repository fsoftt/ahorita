using Ahorita.App.Droid.Notifications;
using Android.App;
using Android.Content;

namespace Ahorita.App.Droid.Reminders;

/// <summary>Botones "En 5 min" y "Hoy no" del recordatorio. "Hoy no" no registra nada: sin culpa.</summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class ReminderActionReceiver : BroadcastReceiver
{
    public const string Snooze = "snooze";
    public const string Dismiss = "dismiss";

    private static readonly TimeSpan SnoozeDelay = TimeSpan.FromMinutes(5);

    public static PendingIntent Intent(Context context, string action, Guid routineId, string name, string emoji)
    {
        var intent = new Intent(context, typeof(ReminderActionReceiver))
            .SetAction("com.fsoftt.ahorita.REMINDER_ACTION." + action)
            .PutExtra(AhoritaNotifications.ExtraAction, action)
            .PutExtra(AhoritaNotifications.ExtraRoutineId, routineId.ToString())
            .PutExtra(AhoritaNotifications.ExtraName, name)
            .PutExtra(AhoritaNotifications.ExtraEmoji, emoji);

        var code = AhoritaNotifications.IdFor(routineId, action == Snooze ? 11 : 12);
        return PendingIntent.GetBroadcast(context, code, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is null || !Guid.TryParse(intent.GetStringExtra(AhoritaNotifications.ExtraRoutineId), out var routineId))
            return;

        AhoritaNotifications.Cancel(context, AhoritaNotifications.IdFor(routineId));

        if (intent.GetStringExtra(AhoritaNotifications.ExtraAction) == Snooze)
        {
            AndroidReminderScheduler.ScheduleSnooze(
                context,
                routineId,
                intent.GetStringExtra(AhoritaNotifications.ExtraName) ?? string.Empty,
                intent.GetStringExtra(AhoritaNotifications.ExtraEmoji) ?? string.Empty,
                SnoozeDelay);
        }
    }
}
