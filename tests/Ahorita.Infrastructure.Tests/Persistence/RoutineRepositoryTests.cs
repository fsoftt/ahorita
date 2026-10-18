using Ahorita.Domain.Routines;
using Ahorita.Infrastructure.Persistence;
using Ahorita.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ahorita.Infrastructure.Tests.Persistence;

/// <summary>Usa SQLite real en memoria y aplica las migraciones, igual que la app.</summary>
public sealed class RoutineRepositoryTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 10, 19, 8, 0, 0, TimeSpan.Zero);

    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private DbContextOptions<AhoritaDbContext> _options = null!;

    public async ValueTask InitializeAsync()
    {
        await _connection.OpenAsync();
        _options = new DbContextOptionsBuilder<AhoritaDbContext>().UseSqlite(_connection).Options;

        await using var db = new AhoritaDbContext(_options);
        await db.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync() => await _connection.DisposeAsync();

    private AhoritaDbContext NewContext() => new(_options);

    [Fact]
    public async Task Round_trips_a_routine_with_schedule_and_ordered_steps()
    {
        var ct = TestContext.Current.CancellationToken;
        var schedule = RoutineSchedule.Create(WeekDays.Weekdays, new TimeOnly(7, 30), TimeSpan.FromMinutes(15)).Value;
        var routine = Routine.Create("Mañana", "🌅", RoutineAccent.Tangerine, schedule, Now).Value;
        var water = routine.AddStep("Agua", "💧", TimeSpan.FromMinutes(1)).Value;
        routine.AddStep("Ducha", "🚿", TimeSpan.FromMinutes(10), isOptional: true);
        routine.AddStep("Vestirte");
        routine.MoveStep(water.Id, 2);

        await using (var db = NewContext())
        {
            new RoutineRepository(db).Add(routine);
            await db.SaveChangesAsync(ct);
        }

        await using (var db = NewContext())
        {
            var loaded = await new RoutineRepository(db).GetByIdAsync(routine.Id, ct);

            loaded.ShouldNotBeNull();
            loaded.Name.ShouldBe("Mañana");
            loaded.Emoji.ShouldBe("🌅");
            loaded.Accent.ShouldBe(RoutineAccent.Tangerine);
            loaded.CreatedAt.ShouldBe(Now);
            loaded.Schedule.ShouldBe(schedule);
            loaded.Steps.Select(s => s.Title).ShouldBe(["Ducha", "Vestirte", "Agua"]);
            loaded.Steps[0].IsOptional.ShouldBeTrue();
            loaded.Steps[0].Duration.ShouldBe(TimeSpan.FromMinutes(10));
            loaded.Steps[1].Duration.ShouldBeNull();
        }
    }

    [Fact]
    public async Task Persists_changes_to_steps_of_a_loaded_routine()
    {
        var ct = TestContext.Current.CancellationToken;
        var routine = Routine.Create("Noche", "🌙", RoutineAccent.Indigo, RoutineSchedule.OnDemand, Now).Value;
        var first = routine.AddStep("Dientes").Value;
        routine.AddStep("Leer");

        await using (var db = NewContext())
        {
            db.Routines.Add(routine);
            await db.SaveChangesAsync(ct);
        }

        await using (var db = NewContext())
        {
            var loaded = (await new RoutineRepository(db).GetByIdAsync(routine.Id, ct))!;
            loaded.RemoveStep(first.Id);
            loaded.AddStep("Luces fuera", "😴");
            await db.SaveChangesAsync(ct);
        }

        await using (var db = NewContext())
        {
            var loaded = (await new RoutineRepository(db).GetByIdAsync(routine.Id, ct))!;
            loaded.Steps.Select(s => s.Title).ShouldBe(["Leer", "Luces fuera"]);
            loaded.Steps.Select(s => s.Position).ShouldBe([0, 1]);
        }
    }

    [Fact]
    public async Task ListActive_excludes_archived_routines()
    {
        var ct = TestContext.Current.CancellationToken;
        var active = Routine.Create("Activa", "✅", RoutineAccent.Mint, RoutineSchedule.OnDemand, Now).Value;
        var archived = Routine.Create("Archivada", "📦", RoutineAccent.Mint, RoutineSchedule.OnDemand, Now).Value;
        archived.Archive();

        await using (var db = NewContext())
        {
            db.Routines.AddRange(active, archived);
            await db.SaveChangesAsync(ct);
        }

        await using (var db = NewContext())
        {
            var list = await new RoutineRepository(db).ListActiveAsync(ct);
            list.Select(r => r.Name).ShouldBe(["Activa"]);
        }
    }

    [Fact]
    public async Task Model_has_no_pending_changes_without_a_migration()
    {
        await using var db = NewContext();

        db.Database.HasPendingModelChanges().ShouldBeFalse(
            "El modelo cambió: genera una migración con 'dotnet ef migrations add'.");
        await Task.CompletedTask;
    }
}
