using System;
using System.Collections.Generic;
using RsseEngine.Dto;
using RsseEngine.Dto.Offsets;
using RsseEngine.Indexes;

namespace RsseEngine.Processor;

/// <summary>
/// BM25 (Best Match 25) — вероятностный алгоритм ранжирования,
/// используемый для определения релевантности документов поисковому запросу.
/// Он является усовершенствованной версией модели TF-IDF
/// </summary>
/// <param name="avgDocumentLength">Средняя длина документа</param>
public class Bm25Calculator(
    Dictionary<Token, InternalDocumentIdList> invertedIndex,
    List<ArrayOffsetTokenVector> directIndex,
    double avgDocumentLength)
{
    /// <summary>
    /// Коэффициент насыщения
    /// </summary>
    private const double K1 = 2.0;

    /// <summary>
    /// Коэффициент длины документа
    /// </summary>
    private const double B = 0.75;

    public static double CalculateAvgDocumentLength(DirectIndex directIndex)
    {
        if (directIndex.Count == 0)
        {
            return 0D;
        }

        var count = 0L;

        foreach (var keyValuePair in directIndex)
        {
            count += keyValuePair.Value.Extended.Count;
        }

        return (double)count / directIndex.Count;
    }

    /// <summary>
    /// Расчет TF компонента
    /// </summary>
    /// <param name="termFrequency"></param>
    /// <param name="documentLength"></param>
    /// <returns></returns>
    private double CalculateTf(int termFrequency, int documentLength)
    {
        return (K1 + 1) * termFrequency /
               (termFrequency + K1 * (1 - B + B * documentLength / avgDocumentLength));
    }

    /// <summary>
    /// Расчет IDF компонента
    /// </summary>
    /// <param name="numDocuments"></param>
    /// <param name="numDocsWithTerm"></param>
    /// <returns></returns>
    private double CalculateIdf(int numDocuments, int numDocsWithTerm)
    {
        return Math.Log((numDocuments - numDocsWithTerm + 0.5) /
                        (numDocsWithTerm + 0.5));
    }

    /// <summary>
    /// Общий расчет BM25 для термина
    /// </summary>
    /// <param name="termFrequency"></param>
    /// <param name="documentLength"></param>
    /// <param name="numDocuments"></param>
    /// <param name="numDocsWithTerm"></param>
    /// <returns></returns>
    public double CalculateBm25(int termFrequency,
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
    /// Вычисление BM25 для слова в документе
    /// </summary>
    /// <param name="token"></param>
    /// <param name="documentLength"></param>
    /// <param name="document"></param>
    /// <param name="tfidf"></param>
    /// <returns></returns>
    private bool TryCalculateBm25(Token token, int documentLength, ArrayOffsetTokenVector document, out double tfidf)
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
    /// Вычисление BM25 вектора токенов для документа
    /// </summary>
    /// <param name="searchVector"></param>
    /// <param name="tokensCount"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    public double CalculateBm25(TokenVector searchVector, int tokensCount, ArrayOffsetTokenVector document)
    {
        var result = 0D;

        foreach (var token in searchVector)
        {
            if (TryCalculateBm25(token, tokensCount, document, out var tfidf))
            {
                result += tfidf;
            }
        }

        return result;
    }
}
