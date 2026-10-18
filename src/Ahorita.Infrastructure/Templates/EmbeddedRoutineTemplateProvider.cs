using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ahorita.Application.Abstractions;
using Ahorita.Application.Routines.Templates;
using Ahorita.Domain.Routines;

namespace Ahorita.Infrastructure.Templates;

/// <summary>
/// Lee las plantillas embebidas (<c>routine-templates.{idioma}.json</c>) según el idioma
/// de la interfaz. Usa serialización generada en compilación para ser compatible con trimming.
/// </summary>
internal sealed class EmbeddedRoutineTemplateProvider : IRoutineTemplateProvider
{
    internal const string FallbackLanguage = "es";
    internal static readonly string[] SupportedLanguages = ["es", "en"];

    private readonly ConcurrentDictionary<string, IReadOnlyList<RoutineTemplate>> _cache = new();

    public IReadOnlyList<RoutineTemplate> GetAll() => GetAll(CultureInfo.CurrentUICulture);

    internal IReadOnlyList<RoutineTemplate> GetAll(CultureInfo culture)
    {
        var language = SupportedLanguages.Contains(culture.TwoLetterISOLanguageName)
            ? culture.TwoLetterISOLanguageName
            : FallbackLanguage;

        return _cache.GetOrAdd(language, Load);
    }

    private static IReadOnlyList<RoutineTemplate> Load(string language)
    {
        var resourceName = $"Ahorita.Templates.routine-templates.{language}.json";
        using var stream = typeof(EmbeddedRoutineTemplateProvider).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"No se encontró el recurso embebido '{resourceName}'.");

        var file = JsonSerializer.Deserialize(stream, TemplateJsonContext.Default.TemplateFile)
            ?? throw new InvalidOperationException($"El recurso '{resourceName}' está vacío.");

        return file.Templates.Select(ToTemplate).ToArray();
    }

    private static RoutineTemplate ToTemplate(TemplateDto dto) => new(
        dto.Key,
        dto.Name,
        dto.Emoji,
        dto.Accent,
        dto.Days,
        dto.StartTime is { } time ? TimeOnly.ParseExact(time, "HH:mm", CultureInfo.InvariantCulture) : null,
        dto.Steps
            .Select(s => new RoutineTemplateStep(s.Title, s.Emoji, s.Minutes is { } m ? TimeSpan.FromMinutes(m) : null, s.Optional))
            .ToArray());

    internal sealed record TemplateFile(IReadOnlyList<TemplateDto> Templates);

    internal sealed record TemplateDto(
        string Key,
        string Name,
        string Emoji,
        RoutineAccent Accent,
        WeekDays Days,
        string? StartTime,
        IReadOnlyList<TemplateStepDto> Steps);

    internal sealed record TemplateStepDto(string Title, string? Emoji, int? Minutes, bool Optional = false);
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    ReadCommentHandling = JsonCommentHandling.Skip)]
[JsonSerializable(typeof(EmbeddedRoutineTemplateProvider.TemplateFile))]
internal sealed partial class TemplateJsonContext : JsonSerializerContext;
