using Ahorita.Application.Routines;
using Ahorita.Application.Tests.Fakes;
using Ahorita.Domain.Routines;
using Microsoft.Extensions.Time.Testing;

namespace Ahorita.Application.Tests.Routines;

public sealed class GetTodayOverviewTests
{
    // Lunes 19 de octubre de 2026.
    private static readonly DateTimeOffset MondayMorning = new(2026, 10, 19, 8, 0, 0, TimeSpan.Zero);

    private readonly InMemoryRoutineRepository _repository = new();
    private readonly FakeTimeProvider _time = new(MondayMorning);

    public GetTodayOverviewTests() => _time.SetLocalTimeZone(TimeZoneInfo.Utc);

    private Task<TodayOverview> Execute() => new GetTodayOverview(_repository, _time).ExecuteAsync(TestContext.Current.CancellationToken);

    [Fact]
    public async Task First_run_has_nothing_featured()
    {
        var overview = await Execute();

        overview.IsFirstRun.ShouldBeTrue();
        overview.Featured.ShouldBeNull();
    }

    [Fact]
    public async Task Features_the_next_scheduled_routine_and_lists_the_rest_in_order()
    {
        _repository.Add(Fakes.Routines.Scheduled("Noche", WeekDays.Everyday, 22, 30));
        _repository.Add(Fakes.Routines.Scheduled("Estudio", WeekDays.Weekdays, 16));
        _repository.Add(Fakes.Routines.Scheduled("Mañana", WeekDays.Weekdays, 9));

        var overview = await Execute();

        overview.Featured!.Name.ShouldBe("Mañana");
        overview.Upcoming.Select(r => r.Name).ShouldBe(["Estudio", "Noche"]);
    }

    [Fact]
    public async Task Hides_routines_whose_time_already_passed()
    {
        _repository.Add(Fakes.Routines.Scheduled("Muy temprano", WeekDays.Everyday, 6));
        _repository.Add(Fakes.Routines.Scheduled("Noche", WeekDays.Everyday, 22));

        var overview = await Execute();

        overview.Featured!.Name.ShouldBe("Noche");
        overview.Upcoming.ShouldBeEmpty();
    }

    [Fact]
    public async Task Keeps_a_routine_in_progress_featured_during_its_estimated_duration()
    {
        // Empezó 7:00 y dura 6 × 15 = 90 min: a las 8:00 sigue siendo "la de ahora".
        _repository.Add(Fakes.Routines.Scheduled("Mañana larga", WeekDays.Everyday, 7, stepMinutes: 15, steps: 6));

        var overview = await Execute();

        overview.Featured!.Name.ShouldBe("Mañana larga");
    }

    [Fact]
    public async Task Short_routines_stay_relevant_for_the_minimum_window()
    {
        _repository.Add(Fakes.Routines.Scheduled("Corta", WeekDays.Everyday, 7, 40, stepMinutes: 1));

        var overview = await Execute();

        overview.Featured!.Name.ShouldBe("Corta");
    }

    [Fact]
    public async Task Ignores_routines_scheduled_on_other_days()
    {
        _repository.Add(Fakes.Routines.Scheduled("Sábado", WeekDays.Saturday, 10));

        var overview = await Execute();

        overview.Featured.ShouldBeNull();
        overview.IsFirstRun.ShouldBeFalse();
    }

    [Fact]
    public async Task Falls_back_to_an_on_demand_routine_when_nothing_is_scheduled()
    {
        _repository.Add(Fakes.Routines.OnDemand("Salir de casa"));
        _repository.Add(Fakes.Routines.OnDemand("Limpieza"));

        var overview = await Execute();

        overview.Featured!.Name.ShouldBe("Limpieza");
        overview.AnyTime.Select(r => r.Name).ShouldBe(["Salir de casa"]);
    }

    [Fact]
    public async Task Skips_routines_that_cannot_start()
    {
        _repository.Add(Fakes.Routines.Scheduled("Borrador", WeekDays.Everyday, 9, steps: 0));
        var archived = Fakes.Routines.Scheduled("Archivada", WeekDays.Everyday, 10);
        archived.Archive();
        _repository.Add(archived);

        var overview = await Execute();

        overview.Featured.ShouldBeNull();
    }
}
