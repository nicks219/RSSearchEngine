using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SimpleEngine.Dto.Common;
using SimpleEngine.Indexes;

namespace SimpleEngine.Algorithms.Legacy;

/// <summary>
/// Части алгоритмов для разработки.
/// </summary>
public class DataProcessorPrototype
{
    /// <summary>
    /// Удалить из вектора N токенов, занимающих максимум в пространстве поиска.
    /// Алгоритм проверен как часть кода ReducedSearchSimple, по нескольким тестам производительность/память не уронил.
    /// Не проверен на всех бенчмарках, поэтому не включен в основной код.
    /// </summary>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="rejectThreshold">Порог оценки длины списка идентификаторов токена.</param>
    /// <param name="tokensCanBeRejected">Сколько токенов можно удалить.</param>
    /// <param name="invertedIndexLegacy">Инвертированный индекс.</param>
    /// <returns>Очищенный вектор.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TokenVector RemoveMostFrequentTokens(
        TokenVector searchVector,
        int rejectThreshold,
        int tokensCanBeRejected,
        InvertedIndexLegacy invertedIndexLegacy)
    {
        var mostFrequentTokens = new HashSet<Token>();
        foreach (var token in searchVector)
        {
            invertedIndexLegacy.TryGetIds(token, out var ids);
            if (ids?.Count > rejectThreshold /*&& --tokensCanBeRejected > 0*/)
            {
                // эти токены необходимо пропустить при поиске в searchVector
                mostFrequentTokens.Add(token);
            }
        }

        var sortedFrequentTokens = mostFrequentTokens
            .OrderByDescending(x => x.Value)
            .Take(tokensCanBeRejected);
        var searchVectorAsList = searchVector.GetAsList();
        foreach (var frequentToken in sortedFrequentTokens)
        {
            searchVectorAsList.RemoveAll(x => x == frequentToken.Value);
        }

        // проглядел: токены удаляются из оригинальной коллекции вектора, нет необходимости возвращать дубликат
        return new TokenVector(searchVectorAsList);
    }
}
