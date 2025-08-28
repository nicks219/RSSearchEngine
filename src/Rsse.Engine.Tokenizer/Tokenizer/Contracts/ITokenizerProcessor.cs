using System;
using System.Collections.Generic;
using RsseEngine.Dto;

namespace RsseEngine.Tokenizer.Contracts;

/// <summary>
/// Контракт функционала токенизации документов.
/// </summary>
public interface ITokenizerProcessor
{
    /// <summary>
    /// Обработать и токенизировать текст.
    /// </summary>
    /// <param name="textTokens">Вектор токенов, представляющий обработанный текст.</param>
    /// <param name="text">Необработанный текст в формате массива строк.</param>
    void TokenizeText(List<int> textTokens, params Span<string> text);

    void TokenizeTextWithPositions(List<TokenWithPosition> textTokens, params Span<string> text);
}
