using Ahorita.Application.Abstractions;

namespace Ahorita.Application.Sessions;

internal static class SessionNotifications
{
    public static Task ReflectAsync(IActiveRoutineNotifier notifier, SessionSnapshot snapshot, CancellationToken cancellationToken) =>
        snapshot.IsActive
            ? notifier.ShowAsync(snapshot, cancellationToken)
            : notifier.ClearAsync(cancellationToken);
}
