using Ahorita.Domain.Routines;

namespace Ahorita.Domain.Tests.Routines;

public sealed class RoutineScheduleTests
{
    [Fact]
    public void No_days_means_on_demand()
    {
        var schedule = RoutineSchedule.Create(WeekDays.None, new TimeOnly(7, 0)).Value;

        schedule.ShouldBe(RoutineSchedule.OnDemand);
        schedule.IsOnDemand.ShouldBeTrue();
        schedule.StartTime.ShouldBeNull();
    }

    [Fact]
    public void Scheduled_routines_require_a_start_time()
    {
        RoutineSchedule.Create(WeekDays.Weekdays, startTime: null).Error.ShouldBe(RoutineErrors.ScheduledWithoutTime);
    }

    [Fact]
    public void Uses_the_default_reminder_lead_time()
    {
        var schedule = RoutineSchedule.Create(WeekDays.Monday, new TimeOnly(7, 30)).Value;

        schedule.ReminderLeadTime.ShouldBe(RoutineSchedule.DefaultReminderLeadTime);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(121)]
    public void Rejects_reminder_lead_time_out_of_range(int minutes)
    {
        RoutineSchedule.Create(WeekDays.Monday, new TimeOnly(7, 30), TimeSpan.FromMinutes(minutes))
            .Error.ShouldBe(RoutineErrors.InvalidReminderLeadTime);
    }

    [Fact]
    public void Rejects_undefined_day_flags()
    {
        RoutineSchedule.Create((WeekDays)256, new TimeOnly(7, 30)).Error.ShouldBe(RoutineErrors.InvalidDays);
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, true)]
    [InlineData(DayOfWeek.Friday, true)]
    [InlineData(DayOfWeek.Saturday, false)]
    public void IsScheduledOn_checks_the_day(DayOfWeek day, bool expected)
    {
        var schedule = RoutineSchedule.Create(WeekDays.Weekdays, new TimeOnly(7, 30)).Value;

        schedule.IsScheduledOn(day).ShouldBe(expected);
    }

    [Fact]
    public void Schedules_with_the_same_values_are_equal()
    {
        var a = RoutineSchedule.Create(WeekDays.Weekend, new TimeOnly(9, 0)).Value;
        var b = RoutineSchedule.Create(WeekDays.Weekend, new TimeOnly(9, 0)).Value;

        a.ShouldBe(b);
    }
}
