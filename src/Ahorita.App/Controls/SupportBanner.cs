using Ahorita.App.Services;
using Plugin.AdMob;

namespace Ahorita.App.Controls;

/// <summary>
/// Banner de "Apoyar con anuncios" para el borde inferior de las pantallas principales.
/// Solo existe si la persona lo activó (o con <see cref="AlwaysShow"/> en la página Donar);
/// apagado, no crea la vista de anuncios ni ocupa espacio.
/// </summary>
public sealed class SupportBanner : ContentView
{
    public static readonly BindableProperty AlwaysShowProperty = BindableProperty.Create(
        nameof(AlwaysShow), typeof(bool), typeof(SupportBanner), false,
        propertyChanged: (bindable, _, _) => ((SupportBanner)bindable).Update());

    private SupportAdsPreference? _preference;

    public SupportBanner()
    {
        IsVisible = false;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public bool AlwaysShow
    {
        get => (bool)GetValue(AlwaysShowProperty);
        set => SetValue(AlwaysShowProperty, value);
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        _preference ??= Handler?.MauiContext?.Services.GetService<SupportAdsPreference>();
        if (_preference is not null)
            _preference.Changed += OnPreferenceChanged;

        Update();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (_preference is not null)
            _preference.Changed -= OnPreferenceChanged;
    }

    private void OnPreferenceChanged(object? sender, EventArgs e) => MainThread.BeginInvokeOnMainThread(Update);

    private void Update()
    {
        var show = AlwaysShow || _preference?.IsEnabled == true;
        if (show && Content is null)
            Content = new BannerAd();
        else if (!show)
            Content = null;

        IsVisible = show;
    }
}
