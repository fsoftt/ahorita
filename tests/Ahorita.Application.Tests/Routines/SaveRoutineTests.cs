using Ahorita.Application.Abstractions;
using Ahorita.Application.Routines;
using Ahorita.Application.Tests.Fakes;
using Ahorita.Domain.Routines;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Ahorita.Application.Tests.Routines;

public sealed class SaveRoutineTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 19, 8, 0, 0, TimeSpan.Zero);

    private readonly InMemoryRoutineRepository _repository = new();
    private readonly IRemindersRefresher _reminders = Substitute.For<IRemindersRefresher>();
    private readonly SaveRoutine _useCase;

    public SaveRoutineTests() => _useCase = new SaveRoutine(_repository, _repository, _reminders, new FakeTimeProvider(Now));

    private static RoutineDraft NewDraft(params StepDraft[] steps) =>
        new(null, "Mañana", "🌅", RoutineAccent.Tangerine, WeekDays.Weekdays, new TimeOnly(7, 0), TimeSpan.FromMinutes(5), steps);

    private static StepDraft Step(string title, Guid? id = null, int? minutes = 5) =>
        new(id, title, null, minutes is { } m ? TimeSpan.FromMinutes(m) : null, IsOptional: false);

    [Fact]
    public async Task Creates_a_new_routine_with_its_steps_and_schedule()
    {
        var result = await _useCase.ExecuteAsync(NewDraft(Step("Agua"), Step("Ducha")), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var routine = _repository.All.ShouldHaveSingleItem();
        routine.Id.ShouldBe(result.Value);
        routine.CreatedAt.ShouldBe(Now);
        routine.Schedule.ReminderLeadTime.ShouldBe(TimeSpan.FromMinutes(5));
        routine.Steps.Select(s => s.Title).ShouldBe(["Agua", "Ducha"]);
        await _reminders.Received(1).RefreshAsync(Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Updates_identity_schedule_and_syncs_steps()
    {
        var created = await _useCase.ExecuteAsync(NewDraft(Step("A"), Step("B"), Step("C")), TestContext.Current.CancellationToken);
        var original = _repository.All.Single();
        var (a, c) = (original.Steps[0].Id, original.Steps[2].Id);

        var edit = new RoutineDraft(
            created.Value, "Noche", "🌙", RoutineAccent.Indigo, WeekDays.None, null, null,
            [Step("C editado", c, minutes: null), Step("Nuevo"), Step("A", a)]);
        var result = await _useCase.ExecuteAsync(edit, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var routine = _repository.All.ShouldHaveSingleItem();
        routine.Name.ShouldBe("Noche");
        routine.Emoji.ShouldBe("🌙");
        routine.Accent.ShouldBe(RoutineAccent.Indigo);
        routine.Schedule.IsOnDemand.ShouldBeTrue();
        routine.Steps.Select(s => s.Title).ShouldBe(["C editado", "Nuevo", "A"]);
        routine.Steps[0].Id.ShouldBe(c);
        routine.Steps[0].Duration.ShouldBeNull();
        routine.Steps[2].Id.ShouldBe(a);
    }

    [Fact]
    public async Task Allows_saving_a_draft_without_steps()
    {
        var result = await _useCase.ExecuteAsync(NewDraft(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        _repository.All.Single().CanStart.ShouldBeFalse();
    }

    [Fact]
    public async Task Does_not_save_when_a_rule_fails()
    {
        var result = await _useCase.ExecuteAsync(NewDraft(Step("Ok"), Step(" ")), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(RoutineErrors.StepTitleRequired);
        _repository.SaveCount.ShouldBe(0);
        await _reminders.DidNotReceive().RefreshAsync(Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rejects_a_scheduled_routine_without_time()
    {
        var draft = NewDraft(Step("A")) with { StartTime = null };

        var result = await _useCase.ExecuteAsync(draft, TestContext.Current.CancellationToken);

        result.Error.ShouldBe(RoutineErrors.ScheduledWithoutTime);
    }

    [Fact]
    public async Task Fails_for_unknown_routines()
    {
        var draft = NewDraft(Step("A")) with { Id = Guid.NewGuid() };

        var result = await _useCase.ExecuteAsync(draft, TestContext.Current.CancellationToken);

        result.Error.ShouldBe(ApplicationErrors.RoutineNotFound);
    }

    [Fact]
    public async Task Draft_round_trips_through_GetRoutineForEditing()
    {
        var created = await _useCase.ExecuteAsync(NewDraft(Step("Agua", minutes: 1), Step("Respirar", minutes: null)), TestContext.Current.CancellationToken);

        var draft = (await new GetRoutineForEditing(_repository).ExecuteAsync(created.Value, TestContext.Current.CancellationToken)).Value;

        draft.Name.ShouldBe("Mañana");
        draft.Days.ShouldBe(WeekDays.Weekdays);
        draft.StartTime.ShouldBe(new TimeOnly(7, 0));
        draft.ReminderLeadTime.ShouldBe(TimeSpan.FromMinutes(5));
        draft.Steps.Select(s => (s.Title, s.Duration)).ShouldBe([("Agua", TimeSpan.FromMinutes(1)), ("Respirar", (TimeSpan?)null)]);
        draft.Steps.ShouldAllBe(s => s.Id != null);
    }
}
