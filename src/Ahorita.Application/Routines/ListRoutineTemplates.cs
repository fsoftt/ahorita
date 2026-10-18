using Ahorita.Application.Abstractions;
using Ahorita.Application.Routines.Templates;

namespace Ahorita.Application.Routines;

public sealed class ListRoutineTemplates(IRoutineTemplateProvider templates)
{
    public Task<IReadOnlyList<RoutineTemplate>> ExecuteAsync() => Task.FromResult(templates.GetAll());
}
