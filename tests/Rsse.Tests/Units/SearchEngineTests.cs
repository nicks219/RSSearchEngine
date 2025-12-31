using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RsseEngine.Dto.Offsets;

namespace Rsse.Tests.Units;

[TestClass]
public class SearchEngineTests
{
    [TestMethod]
    public void DocumentDataPoint_ShouldCreate_EmptyHashMap()
    {
        Dictionary<int, int[]?> source = new();
        const int externalId = 777;
        const int externalCount = 333;
        const DocumentDataPoint.DocumentDataPointSearchType searchType = DocumentDataPoint.DocumentDataPointSearchType.HashMap;

        var documentDataPoint = new DocumentDataPoint(source, externalId, externalCount, searchType);

        Assert.AreEqual(0, documentDataPoint.Count);
        Assert.AreEqual(externalId, documentDataPoint.ExternalId);
        Assert.AreEqual(externalCount, documentDataPoint.ExternalCount);
        Assert.AreEqual(10, documentDataPoint.DataSize);
        Assert.AreEqual(DocumentDataPoint.DocumentDataPointSearchType.HashMap, documentDataPoint.SearchType);

        const int key = 0;

        var result0 = documentDataPoint.ContainsKey(key);
        Assert.IsFalse(result0);

        var result1 = documentDataPoint.ContainsKeyLinearScan(key);
        Assert.IsFalse(result1);

        var result2 = documentDataPoint.ContainsKeyBinarySearch(key);
        Assert.IsFalse(result2);

        var position3 = -1;
        var result3 = documentDataPoint.TryFindNextPositionLinearScan(key, ref position3);
        Assert.IsFalse(result3);
        Assert.AreEqual(-1, position3);

        var position4 = -1;
        var result4 = documentDataPoint.TryFindNextPositionBinarySearch(key, ref position4);
        Assert.IsFalse(result4);
        Assert.AreEqual(-1, position4);

        var result5 = documentDataPoint.TryFindTokenCountLinearScan(key, out var count5);
        Assert.IsFalse(result5);
        Assert.AreEqual(0, count5);

        var result6 = documentDataPoint.TryFindTokenCountBinarySearch(key, out var count6);
        Assert.IsFalse(result6);
        Assert.AreEqual(0, count6);

        var result7 = documentDataPoint.TryGetValue(key, out ReadOnlySpan<int> spanResult7);
        Assert.IsFalse(result7);
        Assert.AreEqual(0, spanResult7.Length);
    }

    [TestMethod]
    public void DocumentDataPoint_ShouldCreate_EmptyBinaryTree()
    {
        Dictionary<int, int[]?> source = new();
        const int externalId = 777;
        const int externalCount = 333;
        const DocumentDataPoint.DocumentDataPointSearchType searchType = DocumentDataPoint.DocumentDataPointSearchType.BinaryTree;

        var documentDataPoint = new DocumentDataPoint(source, externalId, externalCount, searchType);

        Assert.AreEqual(0, documentDataPoint.Count);
        Assert.AreEqual(externalId, documentDataPoint.ExternalId);
        Assert.AreEqual(externalCount, documentDataPoint.ExternalCount);
        Assert.AreEqual(5, documentDataPoint.DataSize);
        Assert.AreEqual(DocumentDataPoint.DocumentDataPointSearchType.BinaryTree, documentDataPoint.SearchType);

        const int key = 0;

        var result0 = documentDataPoint.ContainsKey(key);
        Assert.IsFalse(result0);

        var result1 = documentDataPoint.ContainsKeyLinearScan(key);
        Assert.IsFalse(result1);

        var result2 = documentDataPoint.ContainsKeyBinarySearch(key);
        Assert.IsFalse(result2);

        var position3 = -1;
        var result3 = documentDataPoint.TryFindNextPositionLinearScan(key, ref position3);
        Assert.IsFalse(result3);
        Assert.AreEqual(-1, position3);

        var position4 = -1;
        var result4 = documentDataPoint.TryFindNextPositionBinarySearch(key, ref position4);
        Assert.IsFalse(result4);
        Assert.AreEqual(-1, position4);

        var result5 = documentDataPoint.TryFindTokenCountLinearScan(key, out var count5);
        Assert.IsFalse(result5);
        Assert.AreEqual(0, count5);

        var result6 = documentDataPoint.TryFindTokenCountBinarySearch(key, out var count6);
        Assert.IsFalse(result6);
        Assert.AreEqual(0, count6);

        var result7 = documentDataPoint.TryGetValue(key, out ReadOnlySpan<int> spanResult7);
        Assert.IsFalse(result7);
        Assert.AreEqual(0, spanResult7.Length);
    }
}
