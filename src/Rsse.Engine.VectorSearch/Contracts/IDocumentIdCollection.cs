using RsseEngine.Dto;

namespace RsseEngine.Contracts;

public interface IDocumentIdCollection
{
    void Add(DocumentId docId);

    void Remove(DocumentId documentId);

    bool Contains(DocumentId docId);
}
