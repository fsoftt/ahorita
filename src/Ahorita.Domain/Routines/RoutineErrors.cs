using Ahorita.Domain.Common;

namespace Ahorita.Domain.Routines;

public static class RoutineErrors
{
    public static readonly Error NameRequired = new("Routine.NameRequired", "La rutina necesita un nombre.");
    public static readonly Error NameTooLong = new("Routine.NameTooLong", $"El nombre admite hasta {Routine.MaxNameLength} caracteres.");
    public static readonly Error EmojiRequired = new("Routine.EmojiRequired", "La rutina necesita un emoji.");
    public static readonly Error InvalidDays = new("Routine.InvalidDays", "Los días programados no son válidos.");
    public static readonly Error ScheduledWithoutTime = new("Routine.ScheduledWithoutTime", "Una rutina programada necesita hora de inicio.");
    public static readonly Error InvalidReminderLeadTime = new("Routine.InvalidReminderLeadTime", "El aviso previo debe estar entre 0 minutos y 2 horas.");
    public static readonly Error TooManySteps = new("Routine.TooManySteps", $"Una rutina admite hasta {Routine.MaxSteps} pasos.");
    public static readonly Error StepNotFound = new("Routine.StepNotFound", "El paso no existe en esta rutina.");
    public static readonly Error StepTitleRequired = new("Routine.StepTitleRequired", "El paso necesita un título.");
    public static readonly Error StepTitleTooLong = new("Routine.StepTitleTooLong", $"El título del paso admite hasta {RoutineStep.MaxTitleLength} caracteres.");
    public static readonly Error InvalidStepDuration = new("Routine.InvalidStepDuration", "La duración del paso debe ser mayor a 0 y de máximo 4 horas.");
    public static readonly Error InvalidStepOrder = new("Routine.InvalidStepOrder", "El nuevo orden debe incluir cada paso exactamente una vez.");
    public static readonly Error InvalidStepPosition = new("Routine.InvalidStepPosition", "La posición del paso no es válida.");
}
