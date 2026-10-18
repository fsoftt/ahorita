using Ahorita.Application.Routines.Templates;
using Ahorita.Domain.Routines;

namespace Ahorita.App.Features.Shared;

public sealed record TemplateCardModel(string Key, string Name, string Emoji, RoutineAccent Accent, string Meta)
{
    public static TemplateCardModel From(RoutineTemplate template) => new(
        template.Key,
        template.Name,
        template.Emoji,
        template.Accent,
        RoutineText.Meta(template.Steps.Count, template.EstimatedDuration));
}
