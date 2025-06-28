namespace Rsse.Domain.Exceptions;

/// <summary>
/// Исключение компонента поиска.
/// </summary>
/// <param name="message">Сообщение.</param>
public class RsseTokenizerException(string message) : RsseBaseException(message);
