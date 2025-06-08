using System.Collections.Generic;

namespace SearchEngine.Service.Contracts;

/// <summary>
/// Контракт функционала токенизации заметок.
/// </summary>
public interface ITokenizerProcessor
{
    /// <summary>
    /// Обработать и токенизировать текст.
    /// </summary>
    /// <param name="text">Необработанный текст в формате строки.</param>
    /// <returns>Вектор токенов, представляющий обработанный текст.</returns>
    public List<int> TokenizeText(string text);

    /// <summary>
    /// Вычислить метрику сравнения двух векторов.
    /// </summary>
    /// <param name="targetVector">Вектор, в котором ищем.</param>
    /// <param name="searchVector">Вектор, который ищем.</param>
    /// <returns>Метрика количества совпадений.</returns>
    public int ComputeComparisionMetric(List<int> targetVector, List<int> searchVector);
}
