namespace Ahorita.App.Services;

/// <summary>
/// Identificadores de AdMob de Ahorita (el id de la app va en <c>AdMobAppId</c> del .csproj).
/// Debug y el flavor QA (<c>-p:AhoritaQa=true</c>, el que compila CI) muestran los
/// anuncios de prueba de Google: probar nunca genera impresiones ni clics reales.
/// </summary>
public static class AdMobIds
{
    public const string Banner = "ca-app-pub-5615495156288390/9666750728";
    public const string Rewarded = "ca-app-pub-5615495156288390/2885719952";

#if DEBUG || QA_BUILD
    public const bool UseTestAds = true;
#else
    public const bool UseTestAds = false;
#endif
}
