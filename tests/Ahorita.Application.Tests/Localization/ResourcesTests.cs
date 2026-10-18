using System.Reflection;
using System.Xml.Linq;
using Ahorita.Domain.Common;
using Ahorita.Domain.Routines;

namespace Ahorita.Application.Tests.Localization;

/// <summary>
/// Protege los textos de la app (que vive en un proyecto Android y no se puede referenciar aquí):
/// cada error tiene mensaje amable y español e inglés tienen exactamente las mismas claves.
/// </summary>
public sealed class ResourcesTests
{
    private static readonly string StringsFolder = Path.Combine(FindRepositoryRoot(), "src", "Ahorita.App", "Resources", "Strings");

    [Fact]
    public void Spanish_and_english_have_the_same_keys()
    {
        var spanish = Keys("AppResources.resx");
        var english = Keys("AppResources.en.resx");

        spanish.Except(english).ShouldBeEmpty("Faltan en inglés");
        english.Except(spanish).ShouldBeEmpty("Faltan en español");
    }

    [Fact]
    public void Every_error_code_has_a_friendly_message()
    {
        var keys = Keys("AppResources.resx");
        var missing = AllErrors()
            .Select(e => "Error_" + e.Code.Replace('.', '_'))
            .Where(k => !keys.Contains(k))
            .ToArray();

        missing.ShouldBeEmpty();
    }

    [Fact]
    public void No_value_is_empty()
    {
        foreach (var file in new[] { "AppResources.resx", "AppResources.en.resx" })
        {
            var empty = XDocument.Load(Path.Combine(StringsFolder, file)).Root!
                .Elements("data")
                .Where(d => string.IsNullOrWhiteSpace(d.Element("value")?.Value))
                .Select(d => d.Attribute("name")!.Value)
                .ToArray();

            empty.ShouldBeEmpty(file);
        }
    }

    private static HashSet<string> Keys(string file) =>
        XDocument.Load(Path.Combine(StringsFolder, file)).Root!
            .Elements("data")
            .Select(d => d.Attribute("name")!.Value)
            .ToHashSet();

    private static IEnumerable<Error> AllErrors() =>
        new[] { typeof(Routine).Assembly, typeof(ApplicationErrors).Assembly }
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(f => f.FieldType == typeof(Error))
            .Select(f => (Error)f.GetValue(null)!)
            .Where(e => e != Error.None);

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ahorita.slnx")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new InvalidOperationException("No se encontró la raíz del repositorio.");
    }
}
