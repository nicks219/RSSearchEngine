using System;
using Rsse.Search.Dto;

namespace Rsse.Search;

public interface ISearchAlgorithmSelector<in TSearchType, out TSearchProcessor>
    where TSearchType : Enum
{
    TSearchProcessor GetSearchProcessor(TSearchType searchType);

    void AddVector(DocumentId documentId, TokenVector tokenVector);

    void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector);

    void RemoveVector(DocumentId documentId, TokenVector tokenVector);

    void Clear();
}
