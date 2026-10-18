namespace Ahorita.Application.Abstractions;

/// <summary>Prepara el almacenamiento local (p. ej. migraciones) antes del primer caso de uso.</summary>
public interface IStorageInitializer
{
    Task EnsureInitializedAsync();
}
