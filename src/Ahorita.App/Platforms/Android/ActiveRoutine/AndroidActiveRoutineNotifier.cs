using System.Globalization;
using Ahorita.App.Droid.Notifications;
using Ahorita.App.Resources.Strings;
using Ahorita.App.Theming;
using Ahorita.Application.Abstractions;
using Ahorita.Application.Sessions;
using Android.Content;
using AndroidX.Core.App;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;

namespace Ahorita.App.Droid.ActiveRoutine;

/// <summary>
/// Notificación persistente de la rutina en curso: paso actual, cuenta regresiva del sistema
/// (sin servicio en primer plano: Android dibuja el cronómetro) y botones Listo / Saltar.
/// </summary>
internal sealed partial class AndroidActiveRoutineNotifier(ILogger<AndroidActiveRoutineNotifier> logger) : IActiveRoutineNotifier
{
    public Task ShowAsync(SessionSnapshot session, CancellationToken cancellationToken = default)
    {
        try
        {
            var context = global::Android.App.Application.Context;
            AhoritaNotifications.EnsureChannels(context);
            AhoritaNotifications.Notify(context, AhoritaNotifications.ActiveRoutineNotificationId, Build(context, session));
        }
#pragma warning disable CA1031 // La notificación es un extra: la sesión sigue aunque falle.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            LogFailed(logger, ex);
        }

        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        AhoritaNotifications.Cancel(global::Android.App.Application.Context, AhoritaNotifications.ActiveRoutineNotificationId);
        return Task.CompletedTask;
    }

    private static global::Android.App.Notification Build(Context context, SessionSnapshot session)
    {
        var culture = CultureInfo.CurrentCulture;
        var step = session.CurrentStep;
        var title = string.Format(culture, AppResources.Notif_ActiveTitle, $"{session.RoutineEmoji} {session.RoutineName}".Trim(), session.Position, session.TotalSteps);
        var text = step is null ? string.Empty : $"{step.Emoji} {step.Title}".Trim();

        var builder = new NotificationCompat.Builder(context, AhoritaNotifications.ActiveRoutineChannel);
        builder.SetSmallIcon(Resource.Drawable.ic_notification);
        builder.SetContentTitle(title);
        builder.SetContentText(text);
        builder.SetOngoing(true);
        builder.SetOnlyAlertOnce(true);
        builder.SetSilent(true);
        builder.SetCategory(NotificationCompat.CategoryProgress);
        builder.SetPriority(NotificationCompat.PriorityLow);
        builder.SetColor(AccentPalette.For(session.Accent).End.ToPlatform().ToArgb());
        builder.SetProgress(session.TotalSteps, session.Position - 1, false);
        builder.SetContentIntent(AhoritaNotifications.StartRoutineIntent(context, session.RoutineId));
        builder.AddAction(0, AppResources.Notif_Done, SessionActionReceiver.Intent(context, session.Id, SessionAction.CompleteStep));
        builder.AddAction(0, AppResources.Notif_Skip, SessionActionReceiver.Intent(context, session.Id, SessionAction.SkipStep));

        if (session.IsPaused)
        {
            builder.SetSubText(AppResources.Notif_Paused);
            builder.SetShowWhen(false);
        }
        else if (session.CurrentStepDeadline is { } deadline)
        {
            // Cuenta regresiva dibujada por Android: no hace falta mantener la app despierta.
            builder.SetWhen(deadline.ToUnixTimeMilliseconds());
            builder.SetShowWhen(true);
            builder.SetUsesChronometer(true);
            builder.SetChronometerCountDown(true);
        }
        else
        {
            builder.SetShowWhen(false);
        }

        return builder.Build()!;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "No se pudo mostrar la notificación de la rutina en curso")]
    private static partial void LogFailed(ILogger logger, Exception exception);
}
