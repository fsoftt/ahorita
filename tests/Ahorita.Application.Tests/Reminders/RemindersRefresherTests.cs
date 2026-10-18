using Ahorita.Application.Abstractions;
using Ahorita.Application.Reminders;
using Ahorita.Application.Tests.Fakes;
using Ahorita.Domain.Routines;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Ahorita.Application.Tests.Reminders;

public sealed class RemindersRefresherTests
{
    // Lunes 19 de octubre de 2026, 08:00 UTC.
    private static readonly DateTimeOffset MondayMorning = new(2026, 10, 19, 8, 0, 0, TimeSpan.Zero);

    private readonly InMemoryRoutineRepository _routines = new();
    private readonly IReminderScheduler _scheduler = Substitute.For<IReminderScheduler>();
    private readonly FakeTimeProvider _time = new(MondayMorning);
    private IReadOnlyList<ScheduledReminder> _scheduled = [];

    public RemindersRefresherTests()
    {
        _time.SetLocalTimeZone(TimeZoneInfo.Utc);
        _scheduler
            .When(s => s.ReplaceAllAsync(Arg.Any<IReadOnlyList<ScheduledReminder>>(), Arg.Any<CancellationToken>()))
            .Do(call => _scheduled = call.Arg<IReadOnlyList<ScheduledReminder>>());
    }

    private RemindersRefresher Refresher => new(_routines, _scheduler, _time, NullLogger<RemindersRefresher>.Instance);

    [Fact]
    public async Task Schedules_the_plan_for_active_routines()
    {
        _routines.Add(Fakes.Routines.Scheduled("Estudio", WeekDays.Monday, 16));

        await Refresher.RefreshAsync(cancellationToken: TestContext.Current.CancellationToken);

        _scheduled.Select(r => r.Kind).ShouldBe([ReminderKind.HeadsUp, ReminderKind.Start]);
    }

    [Fact]
    public async Task NotBefore_prevents_rescheduling_an_alarm_that_fired_slightly_early()
    {
        // La alarma de las 08:01:00 llega a las 08:00:59.900: no debe volver a programarse hoy.
        var fireAt = MondayMorning.AddMinutes(1);
        _routines.Add(Fakes.Routines.Scheduled("Mañana", WeekDays.Everyday, 8, 1));
        _time.SetUtcNow(fireAt.AddMilliseconds(-100));

        await Refresher.RefreshAsync(notBefore: fireAt, TestContext.Current.CancellationToken);

        _scheduled.Single(r => r.Kind == ReminderKind.Start).FireAt.ShouldBe(fireAt.AddDays(1));
    }

    [Fact]
    public async Task Scheduler_failures_are_swallowed()
    {
        _routines.Add(Fakes.Routines.Scheduled("Mañana", WeekDays.Everyday, 9));
        _scheduler.ReplaceAllAsync(Arg.Any<IReadOnlyList<ScheduledReminder>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Sin permiso"));

        await Should.NotThrowAsync(() => Refresher.RefreshAsync(cancellationToken: TestContext.Current.CancellationToken));
    }
}
