using System.Collections.ObjectModel;

namespace Ahorita.App.Features.Editor;

/// <summary>Lo que el editor de paso necesita: el paso, la lista a la que pertenece y si es nuevo.</summary>
public sealed record StepEditRequest(StepItem Step, ObservableCollection<StepItem> Owner, bool IsNew);
