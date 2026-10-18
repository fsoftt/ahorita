namespace Ahorita.Domain.Sessions;

public enum SessionStatus
{
    Running,
    Paused,

    /// <summary>Se resolvieron todos los pasos (hechos o saltados).</summary>
    Completed,

    /// <summary>La persona decidió terminar antes. Lo hecho cuenta igual.</summary>
    EndedEarly,
}

public enum StepOutcome
{
    Pending,
    Done,
    Skipped,
}
