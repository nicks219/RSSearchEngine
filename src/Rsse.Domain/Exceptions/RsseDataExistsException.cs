namespace SearchEngine.Exceptions;

/// <summary>
/// Данные уже существуют.
/// </summary>
public class RsseDataExistsException(string message) : RsseBaseException(message);

