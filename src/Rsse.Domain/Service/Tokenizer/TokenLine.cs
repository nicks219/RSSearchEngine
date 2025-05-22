using System.Collections.Generic;

namespace SearchEngine.Service.Tokenizer;

/// <summary>
/// Контейнер с двумя векторами для заметки.
/// </summary>
/// <param name="Extended">Расширенный вектор.</param>
/// <param name="Reduced">Урезанный вектор.</param>
public record TokenLine(
    List<int> Extended,
    List<int> Reduced
);
