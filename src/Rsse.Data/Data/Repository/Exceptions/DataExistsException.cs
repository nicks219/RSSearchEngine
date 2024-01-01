using System;

namespace SearchEngine.Data.Repository.Exceptions;

/// <summary>
/// Данные уже существуют
/// </summary>
public class DataExistsException : Exception
{
    public DataExistsException(string message) : base(message)
    {
    }
}
