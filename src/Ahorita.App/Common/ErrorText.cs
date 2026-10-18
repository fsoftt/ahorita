using System.Globalization;
using Ahorita.App.Resources.Strings;
using Ahorita.Domain.Common;

namespace Ahorita.App.Common;

/// <summary>
/// Traduce errores del dominio a textos amables. La clave del recurso es
/// <c>Error_</c> + el código con guiones bajos (p. ej. <c>Error_Routine_NameRequired</c>).
/// </summary>
public static class ErrorText
{
    public static string For(Error error)
    {
        var key = "Error_" + error.Code.Replace('.', '_');
        return AppResources.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? AppResources.Error_Generic;
    }
}
