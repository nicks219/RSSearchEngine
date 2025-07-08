namespace RsseEngine.Contracts;

/// <summary>
/// Посетитель цикла по коллекции.
/// </summary>
/// <typeparam name="TValue">Тип знпчений в коллекции.</typeparam>
public interface IForEachVisitor<in TValue>
{
    /// <summary>
    /// Вызывается в цикле по коллекции.
    /// </summary>
    /// <param name="value">Значение из коллекции.</param>
    /// <returns>Флаг продолжения цикла (true - продолжаем, false - выходим из цикла)</returns>
    bool Visit(TValue value);
}
