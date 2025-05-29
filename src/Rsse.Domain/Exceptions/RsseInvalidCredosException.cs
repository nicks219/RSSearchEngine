namespace SearchEngine.Exceptions;

/// <summary>
/// Некорректные учетные данные.
/// </summary>
public class RsseInvalidCredosException(string message) : RsseBaseException(message);
