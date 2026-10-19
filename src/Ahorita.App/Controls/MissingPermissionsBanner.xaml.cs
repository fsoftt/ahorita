namespace Ahorita.App.Controls;

/// <summary>
/// Avisa que faltan permisos y los recordatorios no sonarán, con un botón por permiso
/// que abre la pantalla de ajustes correcta. Se usa con <c>BindingContext="{Binding Permissions}"</c>.
/// </summary>
public partial class MissingPermissionsBanner : ContentView
{
    public MissingPermissionsBanner() => InitializeComponent();
}
