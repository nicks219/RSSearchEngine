using RsseEngine.Dto;

namespace RsseEngine.Processor;

/// <summary>
/// Вычисление метрики сравнения двух векторов.
/// </summary>
public static class ScoreCalculator
{
    /// <summary>
    /// Вычислить метрику сравнения двух векторов, для эталонного вектора на основе расширенного набора.
    /// Учитывается последовательность токенов (т.е. "слов").
    /// </summary>
    /// <param name="targetVector">Вектор, в котором ищем.</param>
    /// <param name="searchVector">Вектор, который ищем.</param>
    /// <param name="searchStartIndex">Стартовая позиция для анализа внутри вектора с поисковым запросом.</param>
    /// <returns>Метрика количества совпадений.</returns>
    public static int ComputeOrdered(TokenVector targetVector, TokenVector searchVector, int searchStartIndex = 0)
    {
        // NB "облака лошадки без оглядки облака лошадки без оглядки" в 227 и 270 = 5

        var comparisonScore = 0;

        var startIndex = 0;

        for (var index = (uint)searchStartIndex; index < searchVector.Count; index++)
        {
            var hash = searchVector.ElementAt(index);
            var token = new Token(hash);
            var intersectionIndex = targetVector.IndexOf(token, startIndex);
            if (intersectionIndex == -1)
            {
                continue;
            }

            comparisonScore++;

            startIndex = intersectionIndex + 1;

            if (startIndex >= targetVector.Count)
            {
                break;
            }
        }

        return comparisonScore;
    }

    /// <summary>
    /// Вычислить метрику сравнения двух векторов, для эталонного вектора на основе редуцированного набора.
    /// Последовательность токенов (т.е. "слов") не учитывается.
    /// </summary>
    /// <param name="targetVector">Вектор, в котором ищем.</param>
    /// <param name="searchVector">Вектор, который ищем.</param>
    /// <returns>Метрика количества совпадений.</returns>
    public static int ComputeUnordered(TokenVector targetVector, TokenVector searchVector)
    {
        // NB "я ты он она я ты он она я ты он она" будет найдено почти во всех заметках, необходимо обработать результат

        var comparisionScore = 0;
        foreach (var token in searchVector)
        {
            if (targetVector.Contains(token))
            {
                comparisionScore++;
            }
        }

        return comparisionScore;
    }
}
