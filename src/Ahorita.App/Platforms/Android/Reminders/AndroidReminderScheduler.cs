using Ahorita.App.Droid.Notifications;
using Ahorita.Application.Abstractions;
using Ahorita.Application.Reminders;
using Android.App;
using Android.Content;

namespace Ahorita.App.Droid.Reminders;

/// <summary>
/// Programa los recordatorios con AlarmManager. Usa alarmas exactas si la persona dio
/// permiso ("Alarmas y recordatorios"); si no, alarmas inexactas que Android agrupa.
/// </summary>
internal sealed class AndroidReminderScheduler(IPreferences preferences) : IReminderScheduler
{
    internal const string ReminderAction = "com.fsoftt.ahorita.REMINDER";
    private const string ScheduledCodesKey = "ahorita.alarm.codes";

    public Task ReplaceAllAsync(IReadOnlyList<ScheduledReminder> reminders, CancellationToken cancellationToken = default)
    {
        var context = global::Android.App.Application.Context;
        var alarms = AlarmManagerFrom(context);

        foreach (var code in StoredCodes())
            CancelAlarm(context, alarms, code);

        var codes = new List<int>(reminders.Count);
        foreach (var reminder in reminders)
        {
            var code = AhoritaNotifications.IdFor(reminder.RoutineId, (int)reminder.Kind + 1);
            Schedule(context, alarms, code, reminder);
            codes.Add(code);
        }

        preferences.Set(ScheduledCodesKey, string.Join(',', codes));
        return Task.CompletedTask;
    }

    /// <summary>"En 5 min": un recordatorio de inicio extra, independiente de la planificación.</summary>
    public static void ScheduleSnooze(Context context, Guid routineId, string name, string emoji, TimeSpan delay)
    {
        var at = DateTimeOffset.UtcNow + delay;
        var reminder = new ScheduledReminder(routineId, ReminderKind.Start, at, at, name, emoji);
        Schedule(context, AlarmManagerFrom(context), AhoritaNotifications.IdFor(routineId, 99), reminder);
    }

    public static bool CanScheduleExact(Context context) =>
        !OperatingSystem.IsAndroidVersionAtLeast(31) || AlarmManagerFrom(context).CanScheduleExactAlarms();

    private static void Schedule(Context context, AlarmManager alarms, int code, ScheduledReminder reminder)
    {
        var intent = BuildIntent(context)
            .PutExtra(AhoritaNotifications.ExtraRoutineId, reminder.RoutineId.ToString())
            .PutExtra(AhoritaNotifications.ExtraKind, reminder.Kind.ToString())
            .PutExtra(AhoritaNotifications.ExtraName, reminder.RoutineName)
            .PutExtra(AhoritaNotifications.ExtraEmoji, reminder.RoutineEmoji)
            .PutExtra(AhoritaNotifications.ExtraFireAt, reminder.FireAt.ToUnixTimeMilliseconds())
            .PutExtra(AhoritaNotifications.ExtraStartsAt, reminder.StartsAt.ToUnixTimeMilliseconds());

        var pending = PendingIntent.GetBroadcast(context, code, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
        var triggerAt = reminder.FireAt.ToUnixTimeMilliseconds();

        if (CanScheduleExact(context))
            alarms.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerAt, pending);
        else
            alarms.SetAndAllowWhileIdle(AlarmType.RtcWakeup, triggerAt, pending);
    }

    private static void CancelAlarm(Context context, AlarmManager alarms, int code)
    {
        var pending = PendingIntent.GetBroadcast(context, code, BuildIntent(context), PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable);
        if (pending is null)
            return;

        alarms.Cancel(pending);
        pending.Cancel();
    }

    private static Intent BuildIntent(Context context) =>
        new Intent(context, typeof(ReminderReceiver)).SetAction(ReminderAction);

    private IEnumerable<int> StoredCodes() =>
        preferences.Get(ScheduledCodesKey, string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => int.TryParse(c, out var code) ? code : (int?)null)
            .OfType<int>();

    private static AlarmManager AlarmManagerFrom(Context context) =>
        (AlarmManager)context.GetSystemService(Context.AlarmService)!;
}
