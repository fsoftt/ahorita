using Plugin.AdMob;
using Plugin.AdMob.Services;

namespace Ahorita.App.Services;

internal sealed class AdMobDonateAdService(IRewardedAdService rewardedAds) : IDonateAdService
{
    private static readonly TimeSpan LoadTimeout = TimeSpan.FromSeconds(20);

    public async Task<DonateAdResult> ShowRewardedAdAsync()
    {
        var completion = new TaskCompletionSource<DonateAdResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var ad = rewardedAds.CreateAd();

        void OnLoaded(object? sender, EventArgs e) => MainThread.BeginInvokeOnMainThread(ad.Show);
        void OnFailedToLoad(object? sender, IAdError e) => completion.TrySetResult(DonateAdResult.Unavailable);
        void OnFailedToShow(object? sender, IAdError e) => completion.TrySetResult(DonateAdResult.Unavailable);
        void OnDismissed(object? sender, EventArgs e) => completion.TrySetResult(DonateAdResult.Shown);

        ad.OnAdLoaded += OnLoaded;
        ad.OnAdFailedToLoad += OnFailedToLoad;
        ad.OnAdFailedToShow += OnFailedToShow;
        ad.OnAdDismissed += OnDismissed;

        try
        {
            ad.Load();

            // Si no carga en un tiempo razonable, no dejamos el botón esperando para siempre.
            var finished = await Task.WhenAny(completion.Task, Task.Delay(LoadTimeout));
            return finished == completion.Task ? completion.Task.Result : DonateAdResult.Unavailable;
        }
        finally
        {
            ad.OnAdLoaded -= OnLoaded;
            ad.OnAdFailedToLoad -= OnFailedToLoad;
            ad.OnAdFailedToShow -= OnFailedToShow;
            ad.OnAdDismissed -= OnDismissed;
        }
    }
}
