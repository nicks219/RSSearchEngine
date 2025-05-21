using System.Collections.Generic;

namespace SearchEngine.Domain.Contracts;

/// <summary>
/// Контракт функционала токенизации заметок.
/// </summary>
public interface ITokenizerProcessor
{
    /// <summary>
    /// Подготовить заметку к токенизации.
    /// </summary>
    /// <param name="note">Текст заметки.</param>
    /// <returns>Заметка, разбитая на список обработанных слов.</returns>
    public List<string> PreProcessNote(string note);

    /// <summary>
    /// Токенизировать заметку.
    /// </summary>
    /// <param name="strings">Заметка, разбитая на список обработанных слов.</param>
    /// <returns>Вектор токенов, представляющий заметку.</returns>
    public List<int> TokenizeSequence(IEnumerable<string> strings);

    /// <summary>
    /// Вычислить метрику сравнения двух векторов.
    /// </summary>
    /// <param name="referenceTokens">Эталонный вектор токенов.</param>
    /// <param name="inputTokens">Сравниваемый вектор токенов.</param>
    /// <returns>Метрика о количества совпадений.</returns>
    public int ComputeComparisionMetric(List<int> referenceTokens, IEnumerable<int> inputTokens);
}
