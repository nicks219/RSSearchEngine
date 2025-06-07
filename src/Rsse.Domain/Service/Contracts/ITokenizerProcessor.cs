using System.Collections.Generic;

namespace SearchEngine.Service.Contracts;

/// <summary>
/// Контракт функционала токенизации заметок.
/// </summary>
public interface ITokenizerProcessor
{
    /// <summary>
    /// Подготовить и токенизировать заметку.
    /// </summary>
    /// <param name="note">Текст заметки в виде строки.</param>
    /// <returns>Вектор токенов, представляющий обработанную заметку.</returns>
    public List<int> TokenizeNote(string note);

    /// <summary>
    /// Вычислить метрику сравнения двух векторов.
    /// </summary>
    /// <param name="referenceTokens">Эталонный вектор токенов.</param>
    /// <param name="inputTokens">Сравниваемый вектор токенов.</param>
    /// <returns>Метрика о количества совпадений.</returns>
    public int ComputeComparisionMetric(List<int> referenceTokens, List<int> inputTokens);
}
