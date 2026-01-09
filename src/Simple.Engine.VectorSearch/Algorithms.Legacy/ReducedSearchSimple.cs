using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SimpleEngine.Contracts;
using SimpleEngine.Dto.Common;
using SimpleEngine.Indexes;
using SimpleEngine.Pools;
using SimpleEngine.Processor;

namespace SimpleEngine.Algorithms.Legacy;

/// <summary>
/// "Оригинальный" алгоритм с дополнительным инвертированным индексом.
/// </summary>
public readonly ref struct ReducedSearchSimple : IReducedSearchProcessor
{
    /// <summary>
    /// Общий индекс: идентификатор - вектор.
    /// </summary>
    public required GeneralDirectIndexLegacy GeneralDirectIndexLegacy { private get; init; }

    /// <summary>
    /// Обратный индекс: токен - идентификаторы.
    /// </summary>
    public required InvertedIndexLegacy InvertedIndexLegacy { private get; init; }

    /// <summary>
    /// Пул для коллекций.
    /// </summary>
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(nameof(ReducedSearchLegacy));
        }

        //Dictionary<DocumentId, int> searchSpace = [];
        //var searchSpaceMax = new HashSet<DocumentId>();
        Dictionary<DocumentId, int> searchSpace = TempStoragePool.DocumentIdDictionary.Get();
        HashSet<DocumentId> searchSpaceMax = TempStoragePool.DocumentIdHashSet.Get();

        try
        {
            // создаём пространство поиска (без учета последовательности токенов)
            // значение это по сути баллы, набранные запросом для конкретной заметки
            // при "мусорных" токенах пространство поиска может быть практически равно общему индексу, поэтому считаем статистику
            var threshold = searchVector.Count * 0.6D;
            foreach (Token token in searchVector)
            {
                if (!InvertedIndexLegacy.TryGetValue(token, out var ids))
                {
                    continue;
                }
                
                foreach (var id in ids)
                {
                    if (!searchSpace.TryAdd(id, 1))
                    {
                        var score = ++searchSpace[id];
                        // можно сразу считать "максимум" (выигрыш в duplicates benchmark по времени)
                        // при этом подход как в extended не даст большого ускорения по query бенчмарку
                        // TestData.ResponseLimitLarge = 147
                        if (score >= threshold && searchSpaceMax.Count < 147)
                        {
                            searchSpaceMax.Add(id);
                        }
                    }
                }
            }

            // query: "приключится вдруг вот верный друг выручить"
            // total:49K - search:21K - max:50
            // dupl: 1K - search: 1K - max:1

            // поиск в пространстве поиска reduced
            // todo: баллы совпадений будут аналогично посчитаны повторно - лишняя работа, оптимизируй
            /*foreach (var documentId in searchSpaceMax)
            {
                var tokenLine = GeneralDirectIndexLegacy[documentId];
                metricsCalculator.AppendReducedMetric(searchVector, documentId, tokenLine);
            }*/
            // не даёт большого ускорения по query бенчмарку
            foreach (var documentId in searchSpaceMax)
            {
                var tokenLine = GeneralDirectIndexLegacy[documentId];
                var comparisonScore = searchSpace[documentId];
                metricsCalculator.AppendReducedMetric(searchVector, documentId, tokenLine, comparisonScore);
            }
        }
        finally
        {
            TempStoragePool.DocumentIdHashSet.Return(searchSpaceMax);
            TempStoragePool.DocumentIdDictionary.Return(searchSpace);
        }
    }
}


//////////////////////////////////////////////////////////////////////////////

// можно сразу пересчитать скор (количество совпавших токенов) в метрику

        /*for (var i = 0; i < Math.Min(11, searchSpace.Count); i++)
        {
            var id = sortedSearchSpace.ElementAt(i).Key;
            var score = sortedSearchSpace.ElementAt(i).Value;
            // получается что надо по этой метрике сортировать:
            // либо собирать всё где скор >= searchVector.Count * ReducedCoefficient (0.6)
            var metric = score * (1D / GeneralDirectIndexLegacy[id].Reduced.Count);
            searchSpaceMax.Add(sortedSearchSpace.ElementAt(i).Key);
        }*/

        // сортируем:
        // var sortedSearchSpace = searchSpace.OrderByDescending(x => x.Value).ToList();
        // TestData.ResponseLimitLarge = 147
        /*var searchSpaceMax = new List<DocumentId>();
        var threshold = searchVector.Count * 0.6D;
        for(var i = 0; i < Math.Min(150,sortedSearchSpace.Count); i++)
        {
            var score = sortedSearchSpace.ElementAt(i).Value;
            if (score < threshold)
            {
                break;
            }

            var id = sortedSearchSpace.ElementAt(i).Key;
            searchSpaceMax.Add(id);
        }*/

        // todo: следует изменить оценку набранных баллов для низкорейтинговых запросов
        // метрика некорректно зависит от длины текста. id606(4 совпадения):68 ~ 0.059 | id58(5 совпадений): 185 ~ 0.027
        // если скор >= searchVector.Count * ReducedCoefficient(0.6), то: comparisonScore * (1D / reducedTargetVectorSize)
        // текст, где больше совпадений, получает меньшую метрику из-за большей общей длины (сортировка ответа идёт по метрике)
        // из-за этого не получится взять максимум из пространство поиска, как в extended

        // для reduced собираем все документы, баллы совпадений для которых больше либо равны порогу
        /*var searchSpaceMax = new List<DocumentId>();
        var threshold = searchVector.Count * 0.6D;
        for(var i = 0; i < searchSpace.Count; i++)
        {
            var score = searchSpace.ElementAt(i).Value;
            if (score < threshold)
            {
                continue;
            }

            var id = searchSpace.ElementAt(i).Key;
            searchSpaceMax.Add(id);
            // ограничиваем пространство поиска по TestData.ResponseLimitLarge = 147
            if (searchSpaceMax.Count > 146)//146
            {
                break;
            }
        }*/

        // выбираем результат(ы) с одинаковым максимальным рейтингом (у них не обязательно самая высокая релевантность):
        // UI отдает 10 результатов, данная сортировка - один результат:

        // подход как в extended не даст большого ускорения по query бенчмарку
        /*var max = int.MinValue;
        var searchSpaceMax = new List<DocumentId>();
        foreach (var kv in searchSpace)
        {
            if (kv.Value > max)
            {
                max = kv.Value;
                searchSpaceMax.Clear();
                searchSpaceMax.Add(kv.Key);// = kv.Value;
            }
            else if (kv.Value == max)
            {
                searchSpaceMax.Add(kv.Key);// = kv.Value;
            }
        }*/
