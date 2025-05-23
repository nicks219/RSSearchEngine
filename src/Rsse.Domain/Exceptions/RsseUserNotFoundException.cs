namespace SearchEngine.Exceptions;

/// <summary>
/// Не найден пользователь.
/// </summary>
public class RsseUserNotFoundException(string message) : RsseBaseException(message);
