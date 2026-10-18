using Ahorita.Application.Routines;
using Ahorita.Application.Tests.Fakes;
using Ahorita.Domain.Routines;

namespace Ahorita.Application.Tests.Routines;

public sealed class ListRoutinesTests
{
    [Fact]
    public async Task Lists_scheduled_routines_by_time_then_on_demand_by_name()
    {
        var repository = new InMemoryRoutineRepository();
        repository.Add(Fakes.Routines.OnDemand("Salir de casa"));
        repository.Add(Fakes.Routines.Scheduled("Noche", WeekDays.Everyday, 22));
        repository.Add(Fakes.Routines.OnDemand("Limpieza"));
        repository.Add(Fakes.Routines.Scheduled("Mañana", WeekDays.Weekdays, 7));

        var result = await new ListRoutines(repository).ExecuteAsync(TestContext.Current.CancellationToken);

        result.Select(r => r.Name).ShouldBe(["Mañana", "Noche", "Limpieza", "Salir de casa"]);
    }
}
