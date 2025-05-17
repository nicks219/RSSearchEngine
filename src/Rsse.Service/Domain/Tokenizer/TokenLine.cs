using System.Collections.Generic;

namespace SearchEngine.Domain.Tokenizer;

/// <summary>
/// Контейнер с векторами для заметки.
/// </summary>
public record TokenLine(
    List<int> Extended,
    List<int> Reduced
);
