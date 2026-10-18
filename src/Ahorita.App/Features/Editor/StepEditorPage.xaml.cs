using Ahorita.App.Common;

namespace Ahorita.App.Features.Editor;

public partial class StepEditorPage : AppPage
{
    public StepEditorPage(StepEditorViewModel viewModel)
        : base(viewModel) => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Paso nuevo: el teclado aparece listo para escribir (una decisión menos).
        if (string.IsNullOrEmpty(TitleEntry.Text))
            TitleEntry.Focus();
    }
}
