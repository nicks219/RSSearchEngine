namespace SearchEngine.Tokenizer.Dto;

/// <summary>
/// Контейнер с двумя векторами для заметки.
/// </summary>
/// <param name="Extended">Расширенный вектор.</param>
/// <param name="Reduced">Урезанный вектор.</param>
public sealed record TokenVectors(
    TokenVector Extended,
    TokenVector Reduced
);
