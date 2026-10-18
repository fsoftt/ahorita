using Ahorita.Application.Abstractions;
using Ahorita.Application.Routines;
using Ahorita.Application.Tests.Fakes;
using Ahorita.Domain.Routines;
using NSubstitute;

namespace Ahorita.Application.Tests.Routines;

public sealed class ArchiveRoutineTests
{
    [Fact]
    public async Task Archives_and_refreshes_reminders()
    {
        var repository = new InMemoryRoutineRepository();
        var reminders = Substitute.For<IRemindersRefresher>();
        var routine = Fakes.Routines.Scheduled("Mañana", WeekDays.Everyday, 7);
        repository.Add(routine);

        var result = await new ArchiveRoutine(repository, repository, reminders).ExecuteAsync(routine.Id, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        routine.IsArchived.ShouldBeTrue();
        repository.SaveCount.ShouldBe(1);
        await reminders.Received(1).RefreshAsync(Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Fails_for_unknown_routines()
    {
        var repository = new InMemoryRoutineRepository();

        var result = await new ArchiveRoutine(repository, repository, Substitute.For<IRemindersRefresher>())
            .ExecuteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(ApplicationErrors.RoutineNotFound);
    }

    [Fact]
    public async Task AdjustStepDuration_updates_only_the_duration()
    {
        var repository = new InMemoryRoutineRepository();
        var routine = Fakes.Routines.OnDemand("Mañana", steps: 2);
        repository.Add(routine);
        var step = routine.Steps[1];

        var result = await new AdjustStepDuration(repository, repository)
            .ExecuteAsync(routine.Id, step.Id, TimeSpan.FromMinutes(18), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        routine.Steps[1].Duration.ShouldBe(TimeSpan.FromMinutes(18));
        routine.Steps[1].Title.ShouldBe(step.Title);
        repository.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task AdjustStepDuration_fails_for_unknown_steps()
    {
        var repository = new InMemoryRoutineRepository();
        var routine = Fakes.Routines.OnDemand("Mañana");
        repository.Add(routine);

        var result = await new AdjustStepDuration(repository, repository)
            .ExecuteAsync(routine.Id, Guid.NewGuid(), TimeSpan.FromMinutes(5), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(RoutineErrors.StepNotFound);
    }
}
