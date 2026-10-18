using Ahorita.Application.Abstractions;
using Ahorita.Application.Routines;
using Ahorita.Application.Routines.Templates;
using Ahorita.Application.Tests.Fakes;
using Ahorita.Domain.Routines;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Ahorita.Application.Tests.Routines;

public sealed class CreateRoutineFromTemplateTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 19, 8, 0, 0, TimeSpan.Zero);

    private static readonly RoutineTemplate Morning = new(
        "morning",
        "Rutina de mañana",
        "🌅",
        RoutineAccent.Tangerine,
        WeekDays.Weekdays,
        new TimeOnly(7, 0),
        [
            new RoutineTemplateStep("Agua", "💧", TimeSpan.FromMinutes(1)),
            new RoutineTemplateStep("Medicación", "💊", TimeSpan.FromMinutes(1), IsOptional: true),
            new RoutineTemplateStep("Ducha", "🚿", TimeSpan.FromMinutes(10)),
        ]);

    private readonly InMemoryRoutineRepository _repository = new();
    private readonly IRoutineTemplateProvider _templates = Substitute.For<IRoutineTemplateProvider>();
    private readonly IRemindersRefresher _reminders = Substitute.For<IRemindersRefresher>();
    private readonly CreateRoutineFromTemplate _useCase;

    public CreateRoutineFromTemplateTests()
    {
        _templates.GetAll().Returns([Morning]);
        _useCase = new CreateRoutineFromTemplate(_templates, _repository, _repository, _reminders, new FakeTimeProvider(Now));
    }

    [Fact]
    public async Task Creates_a_routine_with_every_template_step()
    {
        var result = await _useCase.ExecuteAsync("morning", TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var routine = _repository.All.ShouldHaveSingleItem();
        routine.Name.ShouldBe("Rutina de mañana");
        routine.Accent.ShouldBe(RoutineAccent.Tangerine);
        routine.Schedule.Days.ShouldBe(WeekDays.Weekdays);
        routine.Schedule.StartTime.ShouldBe(new TimeOnly(7, 0));
        routine.CreatedAt.ShouldBe(Now);
        routine.Steps.Select(s => s.Title).ShouldBe(["Agua", "Medicación", "Ducha"]);
        routine.Steps[1].IsOptional.ShouldBeTrue();

        result.Value.Id.ShouldBe(routine.Id);
        result.Value.StepCount.ShouldBe(3);
        result.Value.EstimatedDuration.ShouldBe(TimeSpan.FromMinutes(12));
        _repository.SaveCount.ShouldBe(1);
        await _reminders.Received(1).RefreshAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Can_be_used_twice_and_creates_independent_routines()
    {
        await _useCase.ExecuteAsync("morning", TestContext.Current.CancellationToken);
        await _useCase.ExecuteAsync("morning", TestContext.Current.CancellationToken);

        _repository.All.Select(r => r.Id).Distinct().Count().ShouldBe(2);
    }

    [Fact]
    public async Task Fails_for_unknown_templates_without_saving()
    {
        var result = await _useCase.ExecuteAsync("nope", TestContext.Current.CancellationToken);

        result.Error.ShouldBe(ApplicationErrors.TemplateNotFound);
        _repository.All.ShouldBeEmpty();
        _repository.SaveCount.ShouldBe(0);
    }
}
