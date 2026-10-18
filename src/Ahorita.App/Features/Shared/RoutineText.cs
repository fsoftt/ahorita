using System.Globalization;
using Ahorita.App.Resources.Strings;
using Ahorita.Domain.Routines;

namespace Ahorita.App.Features.Shared;

/// <summary>Textos derivados de una rutina, formateados según el idioma actual.</summary>
public static class RoutineText
{
    public static string Meta(int stepCount, TimeSpan estimatedDuration)
    {
        var steps = stepCount == 1
            ? AppResources.Routine_OneStep
            : string.Format(CultureInfo.CurrentCulture, AppResources.Routine_Steps, stepCount);

        if (estimatedDuration <= TimeSpan.Zero)
            return steps;

        var minutes = string.Format(CultureInfo.CurrentCulture, AppResources.Routine_Minutes, (int)Math.Ceiling(estimatedDuration.TotalMinutes));
        return steps + AppResources.Routine_Separator + minutes;
    }

    public static string Schedule(WeekDays days, TimeOnly? startTime)
    {
        if (days == WeekDays.None || startTime is null)
            return AppResources.Routine_OnDemand;

        return Days(days) + AppResources.Routine_Separator + Time(startTime.Value);
    }

    public static string Time(TimeOnly time) => time.ToString("t", CultureInfo.CurrentCulture);

    private static string Days(WeekDays days) => days switch
    {
        WeekDays.Everyday => AppResources.Days_Everyday,
        WeekDays.Weekdays => AppResources.Days_Weekdays,
        WeekDays.Weekend => AppResources.Days_Weekend,
        _ => string.Join(" ", days.ToDaysOfWeek().Select(d => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(d))),
    };
}
