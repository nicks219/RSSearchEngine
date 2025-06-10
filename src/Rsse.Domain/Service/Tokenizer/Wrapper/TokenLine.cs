namespace SearchEngine.Service.Tokenizer.Wrapper;

/// <summary>
/// Контейнер с двумя векторами для заметки.
/// </summary>
/// <param name="Extended">Расширенный вектор.</param>
/// <param name="Reduced">Урезанный вектор.</param>
public record TokenLine(
    TokenVector Extended,
    TokenVector Reduced
);
