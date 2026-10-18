using Ahorita.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ahorita.Infrastructure.Persistence.Configurations;

internal sealed class RoutineSessionConfiguration : IEntityTypeConfiguration<RoutineSession>
{
    public void Configure(EntityTypeBuilder<RoutineSession> builder)
    {
        // SQLite no compara DateTimeOffset en texto: guardarlo como binario (ticks + offset)
        // permite filtrar y ordenar por fecha en la base de datos.
        var binaryDate = new DateTimeOffsetToBinaryConverter();

        builder.ToTable("Sessions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.RoutineId);
        builder.Property(s => s.RoutineName).HasMaxLength(40).IsRequired();
        builder.Property(s => s.RoutineEmoji).HasMaxLength(16).IsRequired();
        builder.Property(s => s.RoutineAccent).HasConversion<string>().HasMaxLength(16);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(s => s.StartedAt).HasConversion(binaryDate);
        builder.Property(s => s.FinishedAt).HasConversion(binaryDate);
        builder.Property(s => s.CurrentStepStartedAt).HasConversion(binaryDate);
        builder.Property(s => s.PausedAt).HasConversion(binaryDate);
        builder.Property(s => s.CurrentStepPausedTotal);

        builder.OwnsMany(s => s.Steps, steps =>
        {
            steps.ToTable("SessionSteps");
            steps.WithOwner().HasForeignKey("SessionId");
            steps.HasKey(s => s.Id);
            steps.Property(s => s.Id).ValueGeneratedNever();
            steps.Property(s => s.SourceStepId);
            steps.Property(s => s.Title).HasMaxLength(60).IsRequired();
            steps.Property(s => s.Emoji).HasMaxLength(16);
            steps.Property(s => s.PlannedDuration);
            steps.Property(s => s.IsOptional);
            steps.Property(s => s.Order);
            steps.Property(s => s.Outcome).HasConversion<string>().HasMaxLength(16);
            steps.Property(s => s.WasPostponed);
            steps.Property(s => s.ActualDuration);
        });

        builder.Navigation(s => s.Steps)
            .HasField("_steps")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.StartedAt);
        builder.HasIndex(s => s.RoutineId);
    }
}
