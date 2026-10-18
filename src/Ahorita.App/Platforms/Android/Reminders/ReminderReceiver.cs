using System.Globalization;
using Ahorita.App.Droid.Notifications;
using Ahorita.App.Resources.Strings;
using Ahorita.Application.Reminders;
using Android.App;
using Android.Content;
using AndroidX.Core.App;

namespace Ahorita.App.Droid.Reminders;

/// <summary>Se dispara con cada alarma: muestra el aviso y programa la siguiente ocurrencia.</summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class ReminderReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is null || !Guid.TryParse(intent.GetStringExtra(AhoritaNotifications.ExtraRoutineId), out var routineId))
            return;

        var kind = Enum.TryParse<ReminderKind>(intent.GetStringExtra(AhoritaNotifications.ExtraKind), out var k) ? k : ReminderKind.Start;
        var name = intent.GetStringExtra(AhoritaNotifications.ExtraName) ?? string.Empty;
        var emoji = intent.GetStringExtra(AhoritaNotifications.ExtraEmoji) ?? string.Empty;
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var fireAt = DateTimeOffset.FromUnixTimeMilliseconds(intent.GetLongExtra(AhoritaNotifications.ExtraFireAt, now));
        var startsAt = DateTimeOffset.FromUnixTimeMilliseconds(intent.GetLongExtra(AhoritaNotifications.ExtraStartsAt, now));

        AhoritaNotifications.EnsureChannels(context);
        var notification = kind == ReminderKind.HeadsUp
            ? BuildHeadsUp(context, routineId, name, emoji, startsAt)
            : BuildStart(context, routineId, name, emoji);
        AhoritaNotifications.Notify(context, AhoritaNotifications.IdFor(routineId), notification);

        // Programar la próxima vez (mañana, la semana que viene…).
        ServiceAccess.RunInBackground(this, async () =>
        {
            if (ServiceAccess.UseCases is { } useCases)
                await useCases.ExecuteAsync((RefreshReminders useCase) => useCase.ExecuteAsync(fireAt)).ConfigureAwait(false);
        });
    }

    private static Notification BuildHeadsUp(Context context, Guid routineId, string name, string emoji, DateTimeOffset startsAt)
    {
        var minutes = Math.Max(1, (int)Math.Round((startsAt - DateTimeOffset.UtcNow).TotalMinutes));
        var title = string.Format(CultureInfo.CurrentCulture, AppResources.Notif_HeadsUpTitle, minutes, $"{emoji} {name}".Trim());

        var builder = new NotificationCompat.Builder(context, AhoritaNotifications.RemindersChannel);
        builder.SetSmallIcon(Resource.Drawable.ic_notification);
        builder.SetContentTitle(title);
        builder.SetContentText(AppResources.Notif_HeadsUpText);
        builder.SetCategory(NotificationCompat.CategoryReminder);
        builder.SetPriority(NotificationCompat.PriorityDefault);
        builder.SetAutoCancel(true);
        builder.SetContentIntent(AhoritaNotifications.OpenAppIntent(context));
        builder.AddAction(0, AppResources.Notif_Start, AhoritaNotifications.StartRoutineIntent(context, routineId));
        return builder.Build()!;
    }

    private static Notification BuildStart(Context context, Guid routineId, string name, string emoji)
    {
        var start = AhoritaNotifications.StartRoutineIntent(context, routineId);

        var builder = new NotificationCompat.Builder(context, AhoritaNotifications.RemindersChannel);
        builder.SetSmallIcon(Resource.Drawable.ic_notification);
        builder.SetContentTitle(string.Format(CultureInfo.CurrentCulture, AppResources.Notif_StartTitle, $"{emoji} {name}".Trim()));
        builder.SetContentText(AppResources.Notif_StartText);
        builder.SetCategory(NotificationCompat.CategoryReminder);
        builder.SetPriority(NotificationCompat.PriorityHigh);
        builder.SetAutoCancel(true);
        builder.SetContentIntent(start);
        builder.AddAction(0, AppResources.Notif_Start, start);
        builder.AddAction(0, AppResources.Notif_Snooze, ReminderActionReceiver.Intent(context, ReminderActionReceiver.Snooze, routineId, name, emoji));
        builder.AddAction(0, AppResources.Notif_NotToday, ReminderActionReceiver.Intent(context, ReminderActionReceiver.Dismiss, routineId, name, emoji));
        return builder.Build()!;
    }
}
