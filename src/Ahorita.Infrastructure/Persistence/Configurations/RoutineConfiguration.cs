using Ahorita.Domain.Routines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ahorita.Infrastructure.Persistence.Configurations;

internal sealed class RoutineConfiguration : IEntityTypeConfiguration<Routine>
{
    public void Configure(EntityTypeBuilder<Routine> builder)
    {
        builder.ToTable("Routines");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Name).HasMaxLength(Routine.MaxNameLength).IsRequired();
        builder.Property(r => r.Emoji).HasMaxLength(16).IsRequired();
        builder.Property(r => r.Accent).HasConversion<string>().HasMaxLength(16);
        builder.Property(r => r.CreatedAt);
        builder.Property(r => r.IsArchived);

        builder.ComplexProperty(r => r.Schedule, schedule =>
        {
            schedule.Property(s => s.Days).HasColumnName("ScheduleDays");
            schedule.Property(s => s.StartTime).HasColumnName("ScheduleStartTime");
            schedule.Property(s => s.ReminderLeadTime).HasColumnName("ScheduleReminderLeadTime");
        });

        builder.OwnsMany(r => r.Steps, steps =>
        {
            steps.ToTable("RoutineSteps");
            steps.WithOwner().HasForeignKey("RoutineId");
            steps.HasKey(s => s.Id);
            steps.Property(s => s.Id).ValueGeneratedNever();
            steps.Property(s => s.Title).HasMaxLength(RoutineStep.MaxTitleLength).IsRequired();
            steps.Property(s => s.Emoji).HasMaxLength(16);
            steps.Property(s => s.Duration);
            steps.Property(s => s.IsOptional);
            steps.Property(s => s.Position);
        });

        builder.Navigation(r => r.Steps)
            .HasField("_steps")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(r => r.IsArchived);
    }
}
