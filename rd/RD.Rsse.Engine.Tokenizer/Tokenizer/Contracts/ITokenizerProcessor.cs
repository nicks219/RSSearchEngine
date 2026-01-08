using System;
using System.Collections.Generic;

namespace RD.RsseEngine.Tokenizer.Contracts;

/// <summary>
/// Контракт функционала токенизации документов.
/// </summary>
public interface ITokenizerProcessor
{
    /// <summary>
    /// Обработать и токенизировать текст.
    /// </summary>
    /// <param name="tokens">Вектор токенов, представляющий обработанный текст.</param>
    /// <param name="text">Необработанный текст в формате массива строк.</param>
    public void TokenizeText(List<int> tokens, params Span<string> text);
}
