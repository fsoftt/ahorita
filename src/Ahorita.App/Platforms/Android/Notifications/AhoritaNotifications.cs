using Android.App;
using Android.Content;
using AndroidX.Core.App;
using Ahorita.App.Resources.Strings;

namespace Ahorita.App.Droid.Notifications;

/// <summary>Canales, identificadores y utilidades comunes de notificaciones.</summary>
internal static class AhoritaNotifications
{
    public const string RemindersChannel = "reminders";
    public const string ActiveRoutineChannel = "active_routine";
    public const string CelebrationsChannel = "celebrations";

    public const int ActiveRoutineNotificationId = 1000;
    public const int CelebrationNotificationId = 1001;

    public const string ExtraRoutineId = "ahorita.routineId";
    public const string ExtraSessionId = "ahorita.sessionId";
    public const string ExtraKind = "ahorita.kind";
    public const string ExtraName = "ahorita.name";
    public const string ExtraEmoji = "ahorita.emoji";
    public const string ExtraFireAt = "ahorita.fireAt";
    public const string ExtraStartsAt = "ahorita.startsAt";
    public const string ExtraAction = "ahorita.action";

    /// <summary>Los canales permiten que la persona controle cada tipo por separado desde Android.</summary>
    public static void EnsureChannels(Context context)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
            return;

        var manager = (NotificationManager)context.GetSystemService(Context.NotificationService)!;

        manager.CreateNotificationChannel(new NotificationChannel(RemindersChannel, AppResources.Channel_Reminders, NotificationImportance.High)
        {
            Description = AppResources.Channel_RemindersDescription,
        });

        var active = new NotificationChannel(ActiveRoutineChannel, AppResources.Channel_ActiveRoutine, NotificationImportance.Low)
        {
            Description = AppResources.Channel_ActiveRoutineDescription,
        };
        active.SetSound(null, null);
        active.EnableVibration(false);
        manager.CreateNotificationChannel(active);

        manager.CreateNotificationChannel(new NotificationChannel(CelebrationsChannel, AppResources.Channel_Celebrations, NotificationImportance.Default)
        {
            Description = AppResources.Channel_CelebrationsDescription,
        });
    }

    /// <summary>Id estable por rutina (Guid.GetHashCode es determinista entre procesos).</summary>
    public static int IdFor(Guid routineId, int salt = 0) => unchecked((routineId.GetHashCode() * 31) + salt) & 0x7FFFFFFF;

    /// <summary>Abre la app directo en el modo Ahora de esa rutina: un toque para empezar.</summary>
    public static PendingIntent StartRoutineIntent(Context context, Guid routineId)
    {
        var intent = new Intent(context, typeof(MainActivity))
            .SetAction("com.fsoftt.ahorita.START_ROUTINE")
            .PutExtra(ExtraRoutineId, routineId.ToString())
            .AddFlags(ActivityFlags.NewTask | ActivityFlags.SingleTop | ActivityFlags.ClearTop);

        return PendingIntent.GetActivity(context, IdFor(routineId, 7), intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    public static PendingIntent OpenAppIntent(Context context)
    {
        var intent = new Intent(context, typeof(MainActivity)).AddFlags(ActivityFlags.NewTask | ActivityFlags.SingleTop);
        return PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    public static void Notify(Context context, int id, Notification notification)
    {
        var manager = NotificationManagerCompat.From(context)!;
        if (!manager.AreNotificationsEnabled())
            return;

        try
        {
            manager.Notify(id, notification);
        }
        catch (Java.Lang.SecurityException)
        {
            // Permiso revocado mientras tanto: no hay nada que mostrar.
        }
    }

    public static void Cancel(Context context, int id) => NotificationManagerCompat.From(context)!.Cancel(id);
}
