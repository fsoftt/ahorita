using Ahorita.Domain.Routines;

namespace Ahorita.Application.Reminders;

/// <summary>
/// Calcula la próxima ocurrencia de cada rutina programada y sus avisos.
/// Solo se programa la siguiente: al dispararse, se vuelve a planificar.
/// </summary>
public static class ReminderPlanner
{
    public static IReadOnlyList<ScheduledReminder> Plan(IEnumerable<Routine> routines, DateTimeOffset now, TimeZoneInfo timeZone)
    {
        var reminders = new List<ScheduledReminder>();

        foreach (var routine in routines.Where(r => r.CanStart && !r.Schedule.IsOnDemand))
        {
            if (NextStart(routine.Schedule, now, timeZone) is not { } start)
                continue;

            var headsUp = start - routine.Schedule.ReminderLeadTime;
            if (routine.Schedule.ReminderLeadTime > TimeSpan.Zero && headsUp > now)
                reminders.Add(new(routine.Id, ReminderKind.HeadsUp, headsUp, routine.Name, routine.Emoji));

            reminders.Add(new(routine.Id, ReminderKind.Start, start, routine.Name, routine.Emoji));
        }

        return reminders.OrderBy(r => r.FireAt).ToArray();
    }

    /// <summary>Primer inicio estrictamente posterior a <paramref name="now"/>, dentro de los próximos 7 días.</summary>
    public static DateTimeOffset? NextStart(RoutineSchedule schedule, DateTimeOffset now, TimeZoneInfo timeZone)
    {
        if (schedule.StartTime is not { } startTime)
            return null;

        var localNow = TimeZoneInfo.ConvertTime(now, timeZone);
        var today = DateOnly.FromDateTime(localNow.DateTime);

        for (var offset = 0; offset <= 7; offset++)
        {
            var day = today.AddDays(offset);
            if (!schedule.IsScheduledOn(day.DayOfWeek))
                continue;

            var localStart = day.ToDateTime(startTime);
            var candidate = new DateTimeOffset(localStart, timeZone.GetUtcOffset(localStart));
            if (candidate > now)
                return candidate;
        }

        return null;
    }
}
