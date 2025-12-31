namespace Rsse.Tests.Units;

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RsseEngine.Dto.Offsets;

[TestClass]
public class SearchEngineTests
{
    [TestMethod]
    public void DocumentDataPoint_EmptyHashMap_ShouldNotThrow()
    {
        Dictionary<int, int[]?> source = new();
        int externalId = 777;
        int externalCount = 333;
        DocumentDataPoint.DocumentDataPointSearchType searchType =
            DocumentDataPoint.DocumentDataPointSearchType.HashMap;

        DocumentDataPoint documentDataPoint = new DocumentDataPoint(source, externalId, externalCount, searchType);

        Assert.AreEqual(0, documentDataPoint.Count);
        Assert.AreEqual(externalId, documentDataPoint.ExternalId);
        Assert.AreEqual(externalCount, documentDataPoint.ExternalCount);
        Assert.AreEqual(10, documentDataPoint.DataSize);
        Assert.AreEqual(DocumentDataPoint.DocumentDataPointSearchType.HashMap, documentDataPoint.SearchType);

        int key = 0;

        bool result0 = documentDataPoint.ContainsKey(key);
        Assert.AreEqual(false, result0);

        bool result1 = documentDataPoint.ContainsKeyLinearScan(key);
        Assert.AreEqual(false, result1);

        bool result2 = documentDataPoint.ContainsKeyBinarySearch(key);
        Assert.AreEqual(false, result2);

        int position3 = -1;
        bool result3 = documentDataPoint.TryFindNextPositionLinearScan(key, ref position3);
        Assert.AreEqual(false, result3);
        Assert.AreEqual(-1, position3);

        int position4 = -1;
        bool result4 = documentDataPoint.TryFindNextPositionBinarySearch(key, ref position4);
        Assert.AreEqual(false, result4);
        Assert.AreEqual(-1, position4);

        bool result5 = documentDataPoint.TryFindTokenCountLinearScan(key, out var count5);
        Assert.AreEqual(false, result5);
        Assert.AreEqual(0, count5);

        bool result6 = documentDataPoint.TryFindTokenCountBinarySearch(key, out var count6);
        Assert.AreEqual(false, result6);
        Assert.AreEqual(0, count6);

        bool result7 = documentDataPoint.TryGetValue(key, out ReadOnlySpan<int> spanResult7);
        Assert.AreEqual(false, result7);
        Assert.AreEqual(0, spanResult7.Length);
    }

    [TestMethod]
    public void DocumentDataPoint_EmptyBinaryTree_ShouldNotThrow()
    {
        Dictionary<int, int[]?> source = new();
        int externalId = 777;
        int externalCount = 333;
        DocumentDataPoint.DocumentDataPointSearchType searchType =
            DocumentDataPoint.DocumentDataPointSearchType.BinaryTree;

        DocumentDataPoint documentDataPoint = new DocumentDataPoint(source, externalId, externalCount, searchType);

        Assert.AreEqual(0, documentDataPoint.Count);
        Assert.AreEqual(externalId, documentDataPoint.ExternalId);
        Assert.AreEqual(externalCount, documentDataPoint.ExternalCount);
        Assert.AreEqual(5, documentDataPoint.DataSize);
        Assert.AreEqual(DocumentDataPoint.DocumentDataPointSearchType.BinaryTree, documentDataPoint.SearchType);

        int key = 0;

        bool result0 = documentDataPoint.ContainsKey(key);
        Assert.AreEqual(false, result0);

        bool result1 = documentDataPoint.ContainsKeyLinearScan(key);
        Assert.AreEqual(false, result1);

        bool result2 = documentDataPoint.ContainsKeyBinarySearch(key);
        Assert.AreEqual(false, result2);

        int position3 = -1;
        bool result3 = documentDataPoint.TryFindNextPositionLinearScan(key, ref position3);
        Assert.AreEqual(false, result3);
        Assert.AreEqual(-1, position3);

        int position4 = -1;
        bool result4 = documentDataPoint.TryFindNextPositionBinarySearch(key, ref position4);
        Assert.AreEqual(false, result4);
        Assert.AreEqual(-1, position4);

        bool result5 = documentDataPoint.TryFindTokenCountLinearScan(key, out var count5);
        Assert.AreEqual(false, result5);
        Assert.AreEqual(0, count5);

        bool result6 = documentDataPoint.TryFindTokenCountBinarySearch(key, out var count6);
        Assert.AreEqual(false, result6);
        Assert.AreEqual(0, count6);

        bool result7 = documentDataPoint.TryGetValue(key, out ReadOnlySpan<int> spanResult7);
        Assert.AreEqual(false, result7);
        Assert.AreEqual(0, spanResult7.Length);
    }
}
