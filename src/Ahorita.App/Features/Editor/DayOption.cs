using System.Globalization;
using Ahorita.Domain.Routines;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ahorita.App.Features.Editor;

public sealed partial class DayOption(DayOfWeek day) : ObservableObject
{
    public DayOfWeek Day { get; } = day;

    public WeekDays Flag => Day.ToWeekDays();

    public string Label => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(Day).ToUpper(CultureInfo.CurrentCulture);

    public string FullName => CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(Day);

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
