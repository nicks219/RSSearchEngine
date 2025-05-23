using System;

namespace SearchEngine.Exceptions;

/// <summary>
/// Базовый класс исключений.
/// </summary>
public class RsseBaseException : Exception
{
    /// <summary>
    /// Создать исключение с сообщением.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    public RsseBaseException(string message) : base(message) { }

    /// <summary>
    /// Создать исключение, оборачивающее нативное исключение.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    /// <param name="ex">Оборачиваемое исключение.</param>
    public RsseBaseException(string message, Exception ex) : base(message, ex) { }
}
