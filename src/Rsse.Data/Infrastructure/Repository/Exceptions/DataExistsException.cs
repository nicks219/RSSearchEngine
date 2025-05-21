using System;

namespace SearchEngine.Infrastructure.Repository.Exceptions;

/// <summary>
/// Данные уже существуют.
/// </summary>
public class DataExistsException(string message) : Exception(message);

