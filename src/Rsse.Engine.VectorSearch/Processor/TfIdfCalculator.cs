using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RsseEngine.Dto;
using RsseEngine.Dto.Offsets;

namespace RsseEngine.Processor;

/// <summary>
/// TF-IDF (Term Frequency-Inverse Document Frequency) — это метод извлечения признаков из текстовых данных,
/// который определяет важность каждого слова в документе относительно всей коллекции текстов.
/// </summary>
/// <param name="invertedIndex"></param>
/// <param name="directIndex"></param>
public class TfIdfCalculator(
    Dictionary<Token, InternalDocumentIdList> invertedIndex,
    List<ArrayOffsetTokenVector> directIndex)
{
    /// <summary>
    /// Расчет TF компонента
    /// </summary>
    /// <param name="termFrequency"></param>
    /// <param name="documentLength"></param>
    /// <returns></returns>
    private static double CalculateTf(int termFrequency, int documentLength)
    {
        return (double)termFrequency / documentLength;
    }

    /// <summary>
    /// Расчет IDF компонента
    /// </summary>
    /// <param name="numDocuments"></param>
    /// <param name="numDocsWithTerm"></param>
    /// <returns></returns>
    private static double CalculateIdf(int numDocuments, int numDocsWithTerm)
    {
        return Math.Log((double)numDocuments / numDocsWithTerm);
    }

    /// <summary>
    /// Общий расчет TF-IDF для термина
    /// </summary>
    /// <param name="termFrequency"></param>
    /// <param name="documentLength"></param>
    /// <param name="numDocuments"></param>
    /// <param name="numDocsWithTerm"></param>
    /// <returns></returns>
    public double CalculateTfIdf(int termFrequency,
        int documentLength,
        int numDocuments,
        int numDocsWithTerm)
    {
        var tf = CalculateTf(termFrequency, documentLength);
        var idf = CalculateIdf(numDocuments, numDocsWithTerm);

        return tf * idf;
    }

    /// <summary>
    /// Вычисление TF для слова в документе
    /// </summary>
    /// <param name="token"></param>
    /// <param name="documentLength"></param>
    /// <param name="document"></param>
    /// <param name="tf"></param>
    /// <returns></returns>
    private bool TryCalculateTf(Token token, int documentLength, ArrayOffsetTokenVector document, out double tf)
    {
        // TryFindTokenCountBinarySearch
        if (document.TryFindTokenCountLinearScan(token, out var termFrequency))
        {
            tf = CalculateTf(termFrequency, documentLength);
            return true;
        }

        tf = 0D;
        return false;
    }

    /// <summary>
    /// Вычисление IDF для слова
    /// </summary>
    /// <param name="token"></param>
    /// <param name="idf"></param>
    /// <returns></returns>
    private bool TryCalculateIdf(Token token, out double idf)
    {
        if (invertedIndex.TryGetValue(token, out var documentsWithToken))
        {
            idf = CalculateIdf(directIndex.Count, documentsWithToken.Count);
            return true;
        }

        idf = 0D;
        return false;
    }

    /// <summary>
    /// Вычисление TF-IDF для слова в документе
    /// </summary>
    /// <param name="token"></param>
    /// <param name="documentLength"></param>
    /// <param name="document"></param>
    /// <param name="tfidf"></param>
    /// <returns></returns>
    private bool TryCalculateTfIdf(Token token, int documentLength, ArrayOffsetTokenVector document, out double tfidf)
    {
        if (TryCalculateIdf(token, out var idf))
        {
            if (TryCalculateTf(token, documentLength, document, out var tf))
            {
                tfidf = tf * idf;
                return true;
            }
        }

        tfidf = 0D;
        return false;
    }

    /// <summary>
    /// Вычисление TF-IDF вектора токенов для документа
    /// </summary>
    /// <param name="searchVector"></param>
    /// <param name="documentLength"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    public double CalculateTfIdf(TokenVector searchVector, int documentLength, ArrayOffsetTokenVector document)
    {
        var result = 0D;

        foreach (var token in searchVector)
        {
            if (TryCalculateTfIdf(token, documentLength, document, out var tfidf))
            {
                result += tfidf;
            }
        }

        return result;
    }

    /// <summary>
    /// Вычисление косинусного сходства
    /// </summary>
    /// <param name="searchVector"></param>
    /// <param name="documentLength"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    public double CalculateCosineSimilarity(TokenVector searchVector, int documentLength, ArrayOffsetTokenVector document)
    {
        var counters = new Dictionary<Token, int>();

        foreach (var token in searchVector)
        {
            ref var counter = ref CollectionsMarshal.GetValueRefOrAddDefault(counters, token, out _);
            counter++;
        }

        var dotProduct = 0D;
        var documentMagnitude = 0D;
        var queryMagnitude = 0D;

        // Находим общие термины
        // Вычисляем скалярное произведение и длины векторов
        foreach (var (token, queryTermFrequency) in counters)
        {
            // Вычисление косинусного сходства
            // TryFindTokenCountBinarySearch
            if (document.TryFindTokenCountLinearScan(token, out var documentTermFrequency))
            {
                var documentTf = (double)documentTermFrequency / documentLength;

                var queryTf = (double)queryTermFrequency / searchVector.Count;

                if (invertedIndex.TryGetValue(token, out var documentsWithToken))
                {
                    // Расчитываем Idf как будто поисковый вектор является документом и лежит в индексе поэтому суммируем
                    var documentIdf = Math.Log((double)(directIndex.Count + 1) / (documentsWithToken.Count + queryTermFrequency));
                    var queryIdf = Math.Log((double)(directIndex.Count + 1) / (documentsWithToken.Count + queryTermFrequency));

                    var documentTfIdf = documentTf * documentIdf;
                    var queryTfIdf = queryTf * queryIdf;

                    dotProduct += documentTfIdf * queryTfIdf;
                    documentMagnitude += Math.Pow(documentTfIdf, 2);
                    queryMagnitude += Math.Pow(queryTfIdf, 2);
                }
            }
        }

        var magnitude = Math.Sqrt(documentMagnitude) * Math.Sqrt(queryMagnitude);

        if (magnitude <= double.Epsilon)
        {
            return 0D;
        }

        return dotProduct / magnitude;
    }
}
