using System;
using System.Collections.Generic;
using SearchEngine.Domain.Tokenizer;

namespace SearchEngine.Domain.Contracts;

/// <summary>
/// Контракт функционала токенизации заметок
/// </summary>
public interface ITokenizerProcessor : IDisposable
{
    /// <summary>
    /// Подготовить заметку к токенизации
    /// </summary>
    /// <param name="note">заметка</param>
    /// <returns>заметка, разбитая на список обработанных слов</returns>
    public List<string> PreProcessNote(string note);

    /// <summary>
    /// Токенизировать заметку
    /// </summary>
    /// <param name="strings">заметка, разбитая на список обработанных слов</param>
    /// <returns>вектор токенов</returns>
    public List<int> TokenizeSequence(IEnumerable<string> strings);

    /// <summary>
    /// Вычислить метрику сравнения двух векторов
    /// </summary>
    /// <param name="referenceTokens">эталонный вектор токенов</param>
    /// <param name="inputTokens">сравниваемый вектор токенов</param>
    /// <returns>метрика, говорящая о количестве совпадений</returns>
    public int ComputeComparisionMetric(List<int> referenceTokens, IEnumerable<int> inputTokens);

    /// <summary>
    /// Выбрать эталонную последовательность символов
    /// </summary>
    /// <param name="consonantChain">параметр выбора последовательности символов</param>
    public void SetupChain(ConsonantChain consonantChain);
}

