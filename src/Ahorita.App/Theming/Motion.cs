namespace Ahorita.App.Theming;

/// <summary>
/// Duraciones con nombre y preferencia global de movimiento. Si la persona desactivó
/// las animaciones del sistema (o activa el modo de bajo estímulo), todo se vuelve instantáneo.
/// </summary>
public static class Motion
{
    public const uint Press = 90;
    public const uint Release = 160;
    public const uint Enter = 300;
    public const uint BreathHalfCycle = 1000;

    public const double PressedScale = 0.95;
    public const double BreathScale = 1.03;

    /// <summary>"Quitar animaciones" del sistema; lo actualiza <c>MainActivity</c> al volver a primer plano.</summary>
    public static bool SystemReduced { get; set; }

    /// <summary>Modo de bajo estímulo elegido en Ajustes.</summary>
    public static bool LowStimulus { get; set; }

    public static bool IsReduced => SystemReduced || LowStimulus;
}
