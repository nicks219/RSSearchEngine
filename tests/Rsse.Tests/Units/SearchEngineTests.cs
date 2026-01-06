using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RsseEngine.Dto.Inverted;
using static RsseEngine.Dto.Inverted.CompactedDictionary;

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
        const DictionaryStorageType searchType =
            DictionaryStorageType.HashTableStorage;

        var documentDataPoint = new CompactedDictionary(source, externalId, externalCount, searchType);

        Assert.AreEqual(0, documentDataPoint.Count);
        Assert.AreEqual(externalId, documentDataPoint.ExternalId);
        Assert.AreEqual(externalCount, documentDataPoint.ExternalCount);
        Assert.AreEqual(10, documentDataPoint.DataSize);
        Assert.AreEqual(DictionaryStorageType.HashTableStorage, documentDataPoint.SearchType);

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
        const DictionaryStorageType searchType =
            DictionaryStorageType.SortedArrayStorage;

        var documentDataPoint = new CompactedDictionary(source, externalId, externalCount, searchType);

        Assert.AreEqual(0, documentDataPoint.Count);
        Assert.AreEqual(externalId, documentDataPoint.ExternalId);
        Assert.AreEqual(externalCount, documentDataPoint.ExternalCount);
        Assert.AreEqual(5, documentDataPoint.DataSize);
        Assert.AreEqual(DictionaryStorageType.SortedArrayStorage, documentDataPoint.SearchType);

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
    public void Enumerator_ShouldReturnAllKeyValuePairs()
    {
        // Arrange
        var source = new Dictionary<int, int[]?>
        {
            [1] = new[] { 10 },
            [2] = new[] { 20, 30 }
        };

        var dict = new CompactedDictionary(
            source, 0, 0, DictionaryStorageType.HashTableStorage);

        var result = new Dictionary<int, int[]>();

        // Act
        foreach (var kvp in dict)
        {
            result[kvp.Key] = kvp.Value;
        }

        // Assert
        Assert.AreEqual(2, result.Count);
        CollectionAssert.AreEqual(new[] { 10 }, result[1]);
        CollectionAssert.AreEqual(new[] { 20, 30 }, result[2]);
    }

    [TestMethod]
    public void TryFindNextPosition_ShouldFindNextPositionGreaterThanGiven()
    {
        // Arrange
        var source = new Dictionary<int, int[]?>
        {
            [1] = new[] { 5, 10, 15, 20 }
        };

        var dict = new CompactedDictionary(
            source, 0, 0, DictionaryStorageType.HashTableStorage);

        int position = 8;

        // Act
        bool found = dict.TryFindNextPositionBinarySearch(1, ref position);

        // Assert
        Assert.IsTrue(found);
        Assert.AreEqual(10, position);
    }

    [TestMethod]
    public void EmptyDictionary_ShouldHaveZeroCount()
    {
        // Arrange
        var emptySource = new Dictionary<int, int[]?>();

        // Act
        var dict = new CompactedDictionary(
            emptySource, 0, 0, DictionaryStorageType.HashTableStorage);

        // Assert
        Assert.AreEqual(0, dict.Count);
        Assert.IsEmpty(dict.Keys);
        Assert.IsEmpty(dict.Values);
    }

    [TestMethod]
    public void Create_WithHashMapStorage_ShouldStoreAndRetrieveValues()
    {
        // Arrange
        var source = new Dictionary<int, int[]?>
        {
            [1] = new[] { 10, 20, 30 },
            [2] = new[] { 40, 50 },
            [3] = Array.Empty<int>()
        };

        // Act
        var dict = new CompactedDictionary(
            source,
            externalId: 100,
            externalCount: 5,
            DictionaryStorageType.HashTableStorage);

        // Assert
        Assert.AreEqual(3, dict.Count);
        Assert.AreEqual(100, dict.ExternalId);
        Assert.AreEqual(5, dict.ExternalCount);
        Assert.AreEqual(DictionaryStorageType.HashTableStorage, dict.SearchType);

        Assert.IsTrue(dict.ContainsKey(1));
        Assert.IsTrue(dict.TryGetValue(1, out int[] value1));
        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, value1);
    }

    [TestMethod]
    public void TryGetValue_WithNonExistentKey_ShouldReturnFalse()
    {
        // Arrange
        var source = new Dictionary<int, int[]?>
        {
            [1] = new[] { 10, 20 }
        };

        var dict = new CompactedDictionary(
            source, 0, 0, DictionaryStorageType.HashTableStorage);

        // Act & Assert
        Assert.IsFalse(dict.ContainsKey(999));
        Assert.IsFalse(dict.TryGetValue(999, out int[] _));
    }

    // --- --- --- --- ---

    [TestMethod]
    // [ПРОБЛЕМА]
    public void Performance_HashMapVsSortedArray_Comparison()
    {
        // Arrange
        var random = new Random(42);
        var source = new Dictionary<int, int[]?>();

        for (var i = 0; i < 1000; i++)
        {
            source[i * 2] = Enumerable.Range(0, random.Next(1, 10))
                .Select(_ => random.Next(1000))
                .ToArray();
        }

        // Act
        var hashMapDict = new CompactedDictionary(
            source, 0, 0, DictionaryStorageType.HashTableStorage);

        var sortedDict = new CompactedDictionary(
            source, 0, 0, DictionaryStorageType.SortedArrayStorage);

        // Assert - оба должны содержать одинаковые данные
        foreach (var key in source.Keys)
        {
            Assert.IsTrue(hashMapDict.ContainsKey(key));
            Assert.IsTrue(sortedDict.ContainsKey(key));

            hashMapDict.TryGetValue(key, out int[] hashMapValue);
            sortedDict.TryGetValue(key, out int[] sortedValue);

            CollectionAssert.AreEqual(hashMapValue, sortedValue);
        }
    }

    [TestMethod]
    // [ПРОБЛЕМА]
    public void SpecialValueHandling_ShortArrays_ShouldBeCompressed()
    {
        // Arrange
        var source = new Dictionary<int, int[]?>
        {
            [1] = [100], // Длина 1 -> сжатие
            [2] = [200, 300], // Длина 2 -> сжатие
            [3] = [400, 500, 600] // Длина >2 -> обычное хранение
        };

        // Act
        var dict = new CompactedDictionary(
            source, 0, 0, DictionaryStorageType.SortedArrayStorage);

        // Assert
        Assert.IsTrue(dict.TryGetValue(1, out int[] value1));
        Assert.IsTrue(dict.TryGetValue(2, out int[] value2));
        Assert.IsTrue(dict.TryGetValue(3, out int[] value3));

        CollectionAssert.AreEqual(new[] { 100 }, value1);
        CollectionAssert.AreEqual(new[] { 200, 300 }, value2);
        CollectionAssert.AreEqual(new[] { 400, 500, 600 }, value3);
    }

    [TestMethod]
    // [ПРОБЛЕМА]
    public void Create_WithSortedArrayStorage_ShouldStoreAndRetrieveValues()
    {
        // Arrange
        var source = new Dictionary<int, int[]?>
        {
            [5] = [100, 200],
            [3] = [300],
            [7] = []
        };

        // Act
        var dict = new CompactedDictionary(
            source,
            externalId: 200,
            externalCount: 3,
            DictionaryStorageType.SortedArrayStorage);

        // Assert
        Assert.AreEqual(3, dict.Count);
        Assert.AreEqual(DictionaryStorageType.SortedArrayStorage, dict.SearchType);

        // Keys should be accessible
        var keys = dict.Keys.ToList();
        CollectionAssert.AreEqual(new[] { 3, 5, 7 }, keys.OrderBy(k => k).ToArray() );
    }

    [TestMethod]
    // [ПРОБЛЕМА]
    public void Equality_TwoIdenticalDictionaries_ShouldBeEqual()
    {
        // Arrange
        var source1 = new Dictionary<int, int[]?>
        {
            [1] = [10, 20]
        };

        var source2 = new Dictionary<int, int[]?>
        {
            [1] = [10, 20]
        };

        // Act
        var dict1 = new CompactedDictionary(source1, 1, 1, DictionaryStorageType.HashTableStorage);
        var dict2 = new CompactedDictionary(source2, 1, 1, DictionaryStorageType.HashTableStorage);

        // Assert
        Assert.AreEqual(dict1, dict2);
        // Assert.IsTrue(dict1 == dict2);
    }
}
