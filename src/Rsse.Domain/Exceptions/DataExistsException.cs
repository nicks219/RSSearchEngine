namespace SearchEngine.Exceptions;

/// <summary>
/// Данные уже существуют.
/// </summary>
public class DataExistsException(string message) : RsseBaseException(message);

