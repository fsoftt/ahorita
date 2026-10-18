using Ahorita.App.Common;

namespace Ahorita.App.Features.Progress;

public partial class ProgressPage : AppPage
{
    private readonly ProgressViewModel _viewModel;

    public ProgressPage(ProgressViewModel viewModel)
        : base(viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Microsoft.Maui.Controls.Application.Current!.RequestedThemeChanged += OnThemeChanged;
    }

    protected override void OnDisappearing()
    {
        Microsoft.Maui.Controls.Application.Current!.RequestedThemeChanged -= OnThemeChanged;
        base.OnDisappearing();
    }

    // Los colores del mapa se calculan con un convertidor: hay que recargar al cambiar de tema.
    private async void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => await _viewModel.OnAppearingAsync();
}
