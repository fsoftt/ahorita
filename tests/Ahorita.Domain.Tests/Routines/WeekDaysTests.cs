using Ahorita.Domain.Routines;

namespace Ahorita.Domain.Tests.Routines;

public sealed class WeekDaysTests
{
    [Fact]
    public void ToDaysOfWeek_starts_on_monday()
    {
        (WeekDays.Sunday | WeekDays.Monday | WeekDays.Wednesday).ToDaysOfWeek()
            .ShouldBe([DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Sunday]);
    }

    [Fact]
    public void Everyday_includes_all_seven_days()
    {
        WeekDays.Everyday.ToDaysOfWeek().Count().ShouldBe(7);
    }

    [Fact]
    public void Weekend_does_not_include_weekdays()
    {
        WeekDays.Weekend.Includes(DayOfWeek.Monday).ShouldBeFalse();
        WeekDays.Weekend.Includes(DayOfWeek.Sunday).ShouldBeTrue();
    }
}
