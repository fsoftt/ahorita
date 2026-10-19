using Ahorita.Application.Reminders;
using Ahorita.Application.Routines;
using Ahorita.Domain.Routines;

namespace Ahorita.Application.Tests.Reminders;

public sealed class MissingPermissionsTests
{
    [Theory]
    [InlineData(true, true, false, false)]
    [InlineData(false, true, true, false)]
    [InlineData(true, false, false, true)]
    [InlineData(false, false, true, true)]
    public void Reports_which_permission_is_missing(bool hasNotifications, bool hasExactAlarms, bool notifications, bool exactAlarms)
    {
        var missing = MissingPermissions.From(hasNotifications, hasExactAlarms);

        missing.Notifications.ShouldBe(notifications);
        missing.ExactAlarms.ShouldBe(exactAlarms);
        missing.Any.ShouldBe(notifications || exactAlarms);
    }

    [Fact]
    public void Shows_nothing_when_no_routine_needs_reminders()
    {
        MissingPermissions.From(false, false).WhenNeeded(false).Any.ShouldBeFalse();
        MissingPermissions.From(false, false).WhenNeeded(true).Any.ShouldBeTrue();
    }

    [Fact]
    public void Is_resolved_only_when_something_was_missing_and_now_nothing_is()
    {
        var granted = MissingPermissions.From(true, true);
        var missing = MissingPermissions.From(false, true);

        granted.IsResolvedFrom(missing).ShouldBeTrue();
        granted.IsResolvedFrom(granted).ShouldBeFalse();
        missing.IsResolvedFrom(granted).ShouldBeFalse();
        missing.IsResolvedFrom(MissingPermissions.From(false, false)).ShouldBeFalse();
    }

    [Fact]
    public void Only_scheduled_routines_with_steps_have_reminders()
    {
        Summary(Fakes.Routines.Scheduled("Mañana", WeekDays.Weekdays, 7)).HasReminders.ShouldBeTrue();
        Summary(Fakes.Routines.Scheduled("Vacía", WeekDays.Weekdays, 7, steps: 0)).HasReminders.ShouldBeFalse();
        Summary(Fakes.Routines.OnDemand("Libre")).HasReminders.ShouldBeFalse();
    }

    private static RoutineSummary Summary(Routine routine) => RoutineSummary.From(routine);
}
