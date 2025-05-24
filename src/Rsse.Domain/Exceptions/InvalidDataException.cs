namespace SearchEngine.Exceptions;

/// <summary>
/// Некорректные данные.
/// </summary>
public class InvalidDataException(string message) : RsseBaseException(message);
