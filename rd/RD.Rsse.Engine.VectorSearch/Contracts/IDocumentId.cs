using System;

namespace RD.RsseEngine.Contracts;

public interface IDocumentId<TDocumentId> : IEquatable<TDocumentId>, IComparable<TDocumentId>
    where TDocumentId : IDocumentId<TDocumentId>
{
    int Value { get; }

    public static abstract bool operator ==(TDocumentId left, TDocumentId right);

    public static abstract bool operator !=(TDocumentId left, TDocumentId right);

    public static abstract bool operator >(TDocumentId left, TDocumentId right);

    public static abstract bool operator <(TDocumentId left, TDocumentId right);

    public static abstract bool operator >=(TDocumentId left, TDocumentId right);

    public static abstract bool operator <=(TDocumentId left, TDocumentId right);
}
