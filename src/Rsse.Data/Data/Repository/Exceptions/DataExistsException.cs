using System;

namespace SearchEngine.Data.Repository.Exceptions;

/// <summary>
/// Данные уже существуют
/// </summary>
public class DataExistsException(string message) : Exception(message);

/// <summary>
/// Некорректные данные
/// </summary>
public class InvalidDataException(string message) : Exception(message);
