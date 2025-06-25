using Rsse.Search.Dto;

namespace Rsse.Search;

public interface IDocumentIdCollection
{
    void Add(DocumentId docId);

    void Remove(DocumentId documentId);

    bool Contains(DocumentId docId);
}
