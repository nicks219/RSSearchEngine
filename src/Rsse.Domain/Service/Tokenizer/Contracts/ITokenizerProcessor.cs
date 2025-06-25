using Rsse.Search.Dto;

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
}
