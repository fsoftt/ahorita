using Ahorita.Domain.Common;

namespace Ahorita.Application;

public static class ApplicationErrors
{
    public static readonly Error TemplateNotFound = new("Template.NotFound", "La plantilla no existe.");
}
