using Ahorita.Domain.Common;

namespace Ahorita.Domain.Sessions;

public static class SessionErrors
{
    public static readonly Error RoutineCannotStart = new("Session.RoutineCannotStart", "La rutina necesita al menos un paso para empezar.");
    public static readonly Error NotActive = new("Session.NotActive", "La sesión ya terminó.");
    public static readonly Error NotPaused = new("Session.NotPaused", "La sesión no está en pausa.");
    public static readonly Error AlreadyPaused = new("Session.AlreadyPaused", "La sesión ya está en pausa.");
    public static readonly Error CannotPostpone = new("Session.CannotPostpone", "Este paso ya se pospuso o es el último.");
    public static readonly Error NotFound = new("Session.NotFound", "La sesión no existe.");
}
