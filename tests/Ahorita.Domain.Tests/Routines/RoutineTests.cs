using Ahorita.Domain.Routines;

namespace Ahorita.Domain.Tests.Routines;

public sealed class RoutineTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 19, 8, 0, 0, TimeSpan.Zero);

    private static Routine NewRoutine(string name = "Mañana") =>
        Routine.Create(name, "🌅", RoutineAccent.Tangerine, RoutineSchedule.OnDemand, Now).Value;

    [Fact]
    public void Create_trims_name_and_emoji()
    {
        var result = Routine.Create("  Mañana  ", " 🌅 ", RoutineAccent.Mint, RoutineSchedule.OnDemand, Now);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Mañana");
        result.Value.Emoji.ShouldBe("🌅");
        result.Value.CreatedAt.ShouldBe(Now);
    }

    [Theory]
    [InlineData("", "🌅", "Routine.NameRequired")]
    [InlineData("   ", "🌅", "Routine.NameRequired")]
    [InlineData("Mañana", "", "Routine.EmojiRequired")]
    public void Create_rejects_missing_identity(string name, string emoji, string expectedCode)
    {
        var result = Routine.Create(name, emoji, RoutineAccent.Mint, RoutineSchedule.OnDemand, Now);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(expectedCode);
    }

    [Fact]
    public void Create_rejects_names_longer_than_the_limit()
    {
        var result = Routine.Create(new string('a', Routine.MaxNameLength + 1), "🌅", RoutineAccent.Mint, RoutineSchedule.OnDemand, Now);

        result.Error.ShouldBe(RoutineErrors.NameTooLong);
    }

    [Fact]
    public void Empty_routine_can_be_saved_but_not_started()
    {
        var routine = NewRoutine();

        routine.CanStart.ShouldBeFalse();

        routine.AddStep("Agua");
        routine.CanStart.ShouldBeTrue();
    }

    [Fact]
    public void Archived_routine_cannot_be_started()
    {
        var routine = NewRoutine();
        routine.AddStep("Agua");

        routine.Archive();

        routine.CanStart.ShouldBeFalse();
    }

    [Fact]
    public void AddStep_appends_steps_in_order()
    {
        var routine = NewRoutine();

        routine.AddStep("Agua", "💧", TimeSpan.FromMinutes(1));
        routine.AddStep("Ducha", "🚿", TimeSpan.FromMinutes(10));

        routine.Steps.Select(s => s.Title).ShouldBe(["Agua", "Ducha"]);
        routine.Steps.Select(s => s.Position).ShouldBe([0, 1]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(241)]
    public void AddStep_rejects_invalid_durations(int minutes)
    {
        var result = NewRoutine().AddStep("Paso", duration: TimeSpan.FromMinutes(minutes));

        result.Error.ShouldBe(RoutineErrors.InvalidStepDuration);
    }

    [Fact]
    public void AddStep_rejects_empty_titles()
    {
        NewRoutine().AddStep("  ").Error.ShouldBe(RoutineErrors.StepTitleRequired);
    }

    [Fact]
    public void AddStep_stops_at_the_hard_limit()
    {
        var routine = NewRoutine();
        for (var i = 0; i < Routine.MaxSteps; i++)
            routine.AddStep($"Paso {i}").IsSuccess.ShouldBeTrue();

        routine.AddStep("Uno más").Error.ShouldBe(RoutineErrors.TooManySteps);
    }

    [Fact]
    public void IsLong_flags_routines_over_the_recommended_size_without_blocking()
    {
        var routine = NewRoutine();
        for (var i = 0; i <= Routine.RecommendedMaxSteps; i++)
            routine.AddStep($"Paso {i}");

        routine.IsLong.ShouldBeTrue();
        routine.CanStart.ShouldBeTrue();
    }

    [Fact]
    public void EstimatedDuration_ignores_steps_without_duration()
    {
        var routine = NewRoutine();
        routine.AddStep("Agua", duration: TimeSpan.FromMinutes(1));
        routine.AddStep("Respirar");
        routine.AddStep("Ducha", duration: TimeSpan.FromMinutes(10));

        routine.EstimatedDuration.ShouldBe(TimeSpan.FromMinutes(11));
    }

    [Fact]
    public void MoveStep_reorders_and_keeps_positions_contiguous()
    {
        var routine = NewRoutine();
        var a = routine.AddStep("A").Value;
        routine.AddStep("B");
        routine.AddStep("C");

        routine.MoveStep(a.Id, 2).IsSuccess.ShouldBeTrue();

        routine.Steps.Select(s => s.Title).ShouldBe(["B", "C", "A"]);
        routine.Steps.Select(s => s.Position).ShouldBe([0, 1, 2]);
    }

    [Fact]
    public void MoveStep_rejects_positions_out_of_range()
    {
        var routine = NewRoutine();
        var a = routine.AddStep("A").Value;

        routine.MoveStep(a.Id, 1).Error.ShouldBe(RoutineErrors.InvalidStepPosition);
    }

    [Fact]
    public void RemoveStep_closes_the_gap()
    {
        var routine = NewRoutine();
        routine.AddStep("A");
        var b = routine.AddStep("B").Value;
        routine.AddStep("C");

        routine.RemoveStep(b.Id).IsSuccess.ShouldBeTrue();

        routine.Steps.Select(s => s.Title).ShouldBe(["A", "C"]);
        routine.Steps.Select(s => s.Position).ShouldBe([0, 1]);
    }

    [Fact]
    public void Operations_on_unknown_steps_fail_gracefully()
    {
        var routine = NewRoutine();
        var unknown = Guid.NewGuid();

        routine.RemoveStep(unknown).Error.ShouldBe(RoutineErrors.StepNotFound);
        routine.MoveStep(unknown, 0).Error.ShouldBe(RoutineErrors.StepNotFound);
        routine.UpdateStep(unknown, "X", null, null, false).Error.ShouldBe(RoutineErrors.StepNotFound);
    }

    [Fact]
    public void UpdateStep_changes_the_step_and_validates()
    {
        var routine = NewRoutine();
        var step = routine.AddStep("Agua").Value;

        routine.UpdateStep(step.Id, " Agua fría ", "🧊", TimeSpan.FromMinutes(2), isOptional: true).IsSuccess.ShouldBeTrue();
        routine.UpdateStep(step.Id, "", null, null, false).Error.ShouldBe(RoutineErrors.StepTitleRequired);

        var updated = routine.Steps.Single();
        updated.Title.ShouldBe("Agua fría");
        updated.Emoji.ShouldBe("🧊");
        updated.Duration.ShouldBe(TimeSpan.FromMinutes(2));
        updated.IsOptional.ShouldBeTrue();
    }

    [Fact]
    public void ChangeAppearance_and_Rename_validate_input()
    {
        var routine = NewRoutine();

        routine.ChangeAppearance("🌙", RoutineAccent.Indigo).IsSuccess.ShouldBeTrue();
        routine.Rename("Noche").IsSuccess.ShouldBeTrue();
        routine.Rename(" ").Error.ShouldBe(RoutineErrors.NameRequired);

        routine.Name.ShouldBe("Noche");
        routine.Emoji.ShouldBe("🌙");
        routine.Accent.ShouldBe(RoutineAccent.Indigo);
    }

    [Fact]
    public void ReorderSteps_applies_a_full_permutation()
    {
        var routine = NewRoutine();
        var a = routine.AddStep("A").Value;
        var b = routine.AddStep("B").Value;
        var c = routine.AddStep("C").Value;

        routine.ReorderSteps([c.Id, a.Id, b.Id]).IsSuccess.ShouldBeTrue();

        routine.Steps.Select(s => s.Title).ShouldBe(["C", "A", "B"]);
        routine.Steps.Select(s => s.Position).ShouldBe([0, 1, 2]);
    }

    [Fact]
    public void ReorderSteps_rejects_partial_duplicated_or_unknown_orders()
    {
        var routine = NewRoutine();
        var a = routine.AddStep("A").Value;
        var b = routine.AddStep("B").Value;

        routine.ReorderSteps([a.Id]).Error.ShouldBe(RoutineErrors.InvalidStepOrder);
        routine.ReorderSteps([a.Id, a.Id]).Error.ShouldBe(RoutineErrors.InvalidStepOrder);
        routine.ReorderSteps([a.Id, Guid.NewGuid()]).Error.ShouldBe(RoutineErrors.InvalidStepOrder);
        routine.Steps.Select(s => s.Id).ShouldBe([a.Id, b.Id]);
    }
}
