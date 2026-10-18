using Ahorita.Application.Reminders;
using Ahorita.Domain.Routines;

namespace Ahorita.Application.Tests.Reminders;

public sealed class ReminderPlannerTests
{
    // Lunes 19 de octubre de 2026, 08:00 UTC.
    private static readonly DateTimeOffset MondayMorning = new(2026, 10, 19, 8, 0, 0, TimeSpan.Zero);

    private static Routine Scheduled(WeekDays days, int hour, int minute = 0, int leadMinutes = 10)
    {
        var schedule = RoutineSchedule.Create(days, new TimeOnly(hour, minute), TimeSpan.FromMinutes(leadMinutes)).Value;
        var routine = Routine.Create("Rutina", "⭐", RoutineAccent.Mint, schedule, MondayMorning).Value;
        routine.AddStep("Paso");
        return routine;
    }

    [Fact]
    public void Plans_heads_up_and_start_for_later_today()
    {
        var plan = ReminderPlanner.Plan([Scheduled(WeekDays.Monday, 9)], MondayMorning, TimeZoneInfo.Utc);

        plan.Select(r => (r.Kind, r.FireAt)).ShouldBe(
        [
            (ReminderKind.HeadsUp, MondayMorning.AddMinutes(50)),
            (ReminderKind.Start, MondayMorning.AddHours(1)),
        ]);
    }

    [Fact]
    public void Moves_to_the_next_scheduled_day_when_today_already_passed()
    {
        var plan = ReminderPlanner.Plan([Scheduled(WeekDays.Monday | WeekDays.Wednesday, 7)], MondayMorning, TimeZoneInfo.Utc);

        plan.Single(r => r.Kind == ReminderKind.Start).FireAt.ShouldBe(new DateTimeOffset(2026, 10, 21, 7, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Weekly_routine_that_already_passed_today_returns_next_week()
    {
        var plan = ReminderPlanner.Plan([Scheduled(WeekDays.Monday, 7)], MondayMorning, TimeZoneInfo.Utc);

        plan.Single(r => r.Kind == ReminderKind.Start).FireAt.ShouldBe(MondayMorning.AddDays(7).AddHours(-1));
    }

    [Fact]
    public void Skips_heads_up_when_it_already_passed_but_keeps_start()
    {
        var plan = ReminderPlanner.Plan([Scheduled(WeekDays.Monday, 8, 5)], MondayMorning, TimeZoneInfo.Utc);

        plan.ShouldHaveSingleItem().Kind.ShouldBe(ReminderKind.Start);
    }

    [Fact]
    public void No_heads_up_when_lead_time_is_zero()
    {
        var plan = ReminderPlanner.Plan([Scheduled(WeekDays.Monday, 9, leadMinutes: 0)], MondayMorning, TimeZoneInfo.Utc);

        plan.ShouldHaveSingleItem().Kind.ShouldBe(ReminderKind.Start);
    }

    [Fact]
    public void Ignores_on_demand_drafts_and_archived_routines()
    {
        var onDemand = Routine.Create("A demanda", "⭐", RoutineAccent.Mint, RoutineSchedule.OnDemand, MondayMorning).Value;
        onDemand.AddStep("Paso");
        var draft = Routine.Create("Borrador", "⭐", RoutineAccent.Mint, RoutineSchedule.Create(WeekDays.Everyday, new TimeOnly(9, 0)).Value, MondayMorning).Value;
        var archived = Scheduled(WeekDays.Everyday, 9);
        archived.Archive();

        ReminderPlanner.Plan([onDemand, draft, archived], MondayMorning, TimeZoneInfo.Utc).ShouldBeEmpty();
    }

    [Fact]
    public void Uses_the_local_time_zone()
    {
        var mexicoCity = TimeZoneInfo.CreateCustomTimeZone("Test-6", TimeSpan.FromHours(-6), "UTC-6", "UTC-6");

        // 08:00 UTC = 02:00 local; la rutina de las 7:00 local es hoy a las 13:00 UTC.
        var plan = ReminderPlanner.Plan([Scheduled(WeekDays.Monday, 7, leadMinutes: 0)], MondayMorning, mexicoCity);

        plan.Single().FireAt.UtcDateTime.ShouldBe(new DateTime(2026, 10, 19, 13, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Orders_reminders_chronologically()
    {
        var plan = ReminderPlanner.Plan(
            [Scheduled(WeekDays.Everyday, 22), Scheduled(WeekDays.Everyday, 9)],
            MondayMorning,
            TimeZoneInfo.Utc);

        plan.Select(r => r.FireAt).ShouldBeInOrder();
    }
}
