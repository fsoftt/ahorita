namespace Ahorita.App.Services;

/// <summary>
/// Identificadores de AdMob de Ahorita. Mientras no estén configurados se usan los
/// anuncios de prueba de Google (nunca generan impresiones reales).
/// El id de la app va además en <c>AdMobAppId</c> del .csproj (AndroidManifest).
/// </summary>
public static class AdMobIds
{
    public static readonly string? Banner;
    public static readonly string? Rewarded;

    // Ids de prueba públicos de Google (https://developers.google.com/admob/android/test-ads).
    private const string TestBanner = "ca-app-pub-3940256099942544/9214589741";
    private const string TestRewarded = "ca-app-pub-3940256099942544/5224354917";

    public static bool IsConfigured => !string.IsNullOrEmpty(Banner) && !string.IsNullOrEmpty(Rewarded);

    public static string BannerOrTest => IsConfigured ? Banner! : TestBanner;

    public static string RewardedOrTest => IsConfigured ? Rewarded! : TestRewarded;
}
