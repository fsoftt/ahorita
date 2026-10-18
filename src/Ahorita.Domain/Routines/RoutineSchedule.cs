using Ahorita.Domain.Common;

namespace Ahorita.Domain.Routines;

/// <summary>
/// Cuándo ocurre una rutina. Sin días programados, la rutina es "a demanda".
/// </summary>
public sealed record RoutineSchedule
{
    public static readonly TimeSpan DefaultReminderLeadTime = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan MaxReminderLeadTime = TimeSpan.FromHours(2);

    public static readonly RoutineSchedule OnDemand = new(WeekDays.None, null, TimeSpan.Zero);

    private RoutineSchedule(WeekDays days, TimeOnly? startTime, TimeSpan reminderLeadTime)
    {
        Days = days;
        StartTime = startTime;
        ReminderLeadTime = reminderLeadTime;
    }

    public WeekDays Days { get; }
    public TimeOnly? StartTime { get; }

    /// <summary>Cuánto antes del inicio llega el aviso previo.</summary>
    public TimeSpan ReminderLeadTime { get; }

    public bool IsOnDemand => Days == WeekDays.None;

    public static Result<RoutineSchedule> Create(WeekDays days, TimeOnly? startTime, TimeSpan? reminderLeadTime = null)
    {
        if (days == WeekDays.None)
            return OnDemand;

        if ((days & ~WeekDays.Everyday) != 0)
            return RoutineErrors.InvalidDays;

        if (startTime is null)
            return RoutineErrors.ScheduledWithoutTime;

        var leadTime = reminderLeadTime ?? DefaultReminderLeadTime;
        if (leadTime < TimeSpan.Zero || leadTime > MaxReminderLeadTime)
            return RoutineErrors.InvalidReminderLeadTime;

        return new RoutineSchedule(days, startTime, leadTime);
    }

    public bool IsScheduledOn(DayOfWeek day) => Days.Includes(day);
}
