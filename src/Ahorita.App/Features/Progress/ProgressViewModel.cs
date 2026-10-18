using Ahorita.App.Common;
using Ahorita.App.Services;
using Microsoft.Extensions.Logging;

namespace Ahorita.App.Features.Progress;

/// <summary>Historial amable (Fase 4). Por ahora solo presenta la idea.</summary>
public sealed class ProgressViewModel(IToastService toast, ILogger<ProgressViewModel> logger) : ViewModelBase(toast, logger);
