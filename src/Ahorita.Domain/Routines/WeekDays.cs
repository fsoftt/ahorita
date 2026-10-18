namespace Ahorita.Domain.Routines;

/// <summary>Días de la semana combinables (a diferencia de <see cref="DayOfWeek"/>).</summary>
[Flags]
public enum WeekDays
{
    None = 0,
    Monday = 1 << 0,
    Tuesday = 1 << 1,
    Wednesday = 1 << 2,
    Thursday = 1 << 3,
    Friday = 1 << 4,
    Saturday = 1 << 5,
    Sunday = 1 << 6,

    Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,
    Weekend = Saturday | Sunday,
    Everyday = Weekdays | Weekend,
}

public static class WeekDaysExtensions
{
    public static WeekDays ToWeekDays(this DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => WeekDays.Monday,
        DayOfWeek.Tuesday => WeekDays.Tuesday,
        DayOfWeek.Wednesday => WeekDays.Wednesday,
        DayOfWeek.Thursday => WeekDays.Thursday,
        DayOfWeek.Friday => WeekDays.Friday,
        DayOfWeek.Saturday => WeekDays.Saturday,
        DayOfWeek.Sunday => WeekDays.Sunday,
        _ => throw new ArgumentOutOfRangeException(nameof(day), day, null),
    };

    public static bool Includes(this WeekDays days, DayOfWeek day) => (days & day.ToWeekDays()) != 0;

    /// <summary>Días incluidos, empezando por el lunes.</summary>
    public static IEnumerable<DayOfWeek> ToDaysOfWeek(this WeekDays days)
    {
        DayOfWeek[] mondayFirst =
        [
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
            DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday,
        ];
        return mondayFirst.Where(day => days.Includes(day));
    }
}
