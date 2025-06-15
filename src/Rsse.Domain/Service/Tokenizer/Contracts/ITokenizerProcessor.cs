using SearchEngine.Service.Tokenizer.Dto;

namespace SearchEngine.Service.Tokenizer.Contracts;

/// <summary>
/// Контракт функционала токенизации заметок.
/// </summary>
public interface ITokenizerProcessor
{
    /// <summary>
    /// Обработать и токенизировать текст.
    /// </summary>
    /// <param name="text">Необработанный текст в формате массива строк.</param>
    /// <returns>Вектор токенов, представляющий обработанный текст.</returns>
    public TokenVector TokenizeText(params string[] text);

    /// <summary>
    /// Обработать и токенизировать текст.
    /// </summary>
    /// <param name="words">Необработанный текст в формате строки.</param>
    /// <returns>Вектор токенов, представляющий обработанный текст.</returns>
    public TokenVector TokenizeText(string words);

    /// <summary>
    /// Вычислить метрику сравнения двух векторов.
    /// </summary>
    /// <param name="targetVector">Вектор, в котором ищем.</param>
    /// <param name="searchVector">Вектор, который ищем.</param>
    /// <param name="searchStartIndex">Позиция, с которой следует начинать анализ по вектору с поисковым запросом.</param>
    /// <returns>Метрика количества совпадений.</returns>
    public int ComputeComparisonScore(TokenVector targetVector, TokenVector searchVector, int searchStartIndex = 0);
}
