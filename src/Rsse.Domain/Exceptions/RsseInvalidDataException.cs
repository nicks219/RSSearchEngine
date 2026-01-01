namespace Rsse.Domain.Exceptions;

/// <summary>
/// Некорректные данные.
/// </summary>
public class RsseInvalidDataException(string message) : RsseBaseException(message);
