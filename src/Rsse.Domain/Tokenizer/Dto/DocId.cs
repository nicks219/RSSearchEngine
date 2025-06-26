using System;

namespace SearchEngine.Tokenizer.Dto;

/// <summary>
/// Идентификатор заметки.
/// В данной версии соответствует идентификатору из базы данных.
/// </summary>
/// <param name="id"></param>
public readonly struct DocId(int id) : IEquatable<DocId>
{
    // Идентификатор заметки.
    private readonly int _id = id;

    /// <summary>
    /// Получить значение идентификатора заметки.
    /// </summary>
    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    internal int Value => _id;

    public bool Equals(DocId other) => _id.Equals(other._id);

    public override bool Equals(object? obj) => obj is DocId other && Equals(other);

    public override int GetHashCode() => _id;

    public static bool operator ==(DocId left, DocId right) => left.Equals(right);

    public static bool operator !=(DocId left, DocId right) => !(left == right);
}
