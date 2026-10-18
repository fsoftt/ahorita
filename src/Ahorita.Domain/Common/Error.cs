namespace Ahorita.Domain.Common;

/// <summary>
/// Error esperado del dominio. El <see cref="Code"/> es estable y sirve para
/// traducir el mensaje en la capa de presentación.
/// </summary>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
