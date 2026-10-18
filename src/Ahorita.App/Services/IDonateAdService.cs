namespace Ahorita.App.Services;

public enum DonateAdResult
{
    /// <summary>El anuncio se mostró y la persona lo cerró: ¡propina recibida!</summary>
    Shown,

    /// <summary>No hubo anuncio disponible (sin conexión, sin inventario…).</summary>
    Unavailable,
}

/// <summary>Muestra un anuncio bonificado como "propina" desde la página Donar.</summary>
public interface IDonateAdService
{
    Task<DonateAdResult> ShowRewardedAdAsync();
}
