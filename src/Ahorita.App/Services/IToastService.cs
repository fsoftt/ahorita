namespace Ahorita.App.Services;

/// <summary>Mensajes breves y no bloqueantes para la persona.</summary>
public interface IToastService
{
    Task ShowAsync(string message);
}
