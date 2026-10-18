using Ahorita.App.Common;
using Ahorita.App.Resources.Strings;
using Ahorita.App.Services;
using Ahorita.App.Theming;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Ahorita.App.Features.Donate;

/// <summary>Donar sin suscripciones: tocar "Ver anuncio" muestra un anuncio corto como propina.</summary>
public sealed partial class DonateViewModel(
    IDonateAdService donateAds,
    INavigationService navigation,
    IToastService toast,
    ILogger<DonateViewModel> logger) : ViewModelBase(toast, logger)
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSeeAd))]
    public partial bool IsAdLoading { get; set; }

    public bool CanSeeAd => !IsAdLoading;

    [RelayCommand]
    private Task SeeAdAsync() => RunSafelyAsync(async () =>
    {
        IsAdLoading = true;
        try
        {
            var result = await donateAds.ShowRewardedAdAsync();
            if (result == DonateAdResult.Shown)
            {
                _ = Haptics.CelebrateAsync();
                await Toast.ShowAsync(AppResources.Donate_Thanks);
            }
            else
            {
                await Toast.ShowAsync(AppResources.Donate_NoAd);
            }
        }
        finally
        {
            IsAdLoading = false;
        }
    });

    [RelayCommand]
    private Task CloseAsync() => navigation.GoBackAsync();
}
