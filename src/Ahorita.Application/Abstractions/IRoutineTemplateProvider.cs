using Ahorita.Application.Routines.Templates;

namespace Ahorita.Application.Abstractions;

/// <summary>Plantillas de rutina listas para usar, en el idioma de la persona.</summary>
public interface IRoutineTemplateProvider
{
    IReadOnlyList<RoutineTemplate> GetAll();
}
