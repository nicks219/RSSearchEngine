using System.Collections.Generic;

namespace SearchEngine.Service.Tokenizer.Dto;

/// <summary>
/// Контейнер с методами, меняющими состояние <see cref="DocIdVector"/>.
/// </summary>
public readonly struct DocIdVectorBuilder
{
    private readonly HashSet<DocId> _vector;

    /// <summary>
    /// Обернуть вектор билдером.
    /// </summary>
    /// <param name="vector">Вектор.</param>
    internal DocIdVectorBuilder(HashSet<DocId> vector)
    {
        _vector = vector;
    }

    /// <summary>
    /// Добавить идентификатор документа к вектору.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    public DocIdVectorBuilder Add(DocId docId)
    {
        _vector.Add(docId);
        return this;
    }

    /// <summary>
    /// Удалить из текущего вектора все элементы второго.
    /// </summary>
    /// <param name="other">Вектор, элементы которого удаляются из первого.</param>
    public DocIdVectorBuilder ExceptWith(DocIdVector other)
    {
        _vector.ExceptWith(other.Vector);
        return this;
    }

    /// <summary>
    /// Собрать вектор.
    /// </summary>
    /// <returns>Вектор.</returns>
    public DocIdVector Build() => new(_vector);
}
