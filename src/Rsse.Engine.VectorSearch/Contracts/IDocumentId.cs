using System;

namespace RsseEngine.Contracts;

public interface IDocumentId<TDocumentId> : IEquatable<TDocumentId>, IComparable<TDocumentId>
    where TDocumentId : IDocumentId<TDocumentId>
{
    int Value { get; }
}
