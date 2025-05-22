using System;

namespace SearchEngine.Infrastructure.Repository.Exceptions;

/// <summary>
/// Некорректные данные.
/// </summary>
public class InvalidDataException(string message) : Exception(message);
