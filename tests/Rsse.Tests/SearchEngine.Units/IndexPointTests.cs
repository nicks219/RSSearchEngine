using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleEngine.Dto.Inverted;
using static SimpleEngine.Dto.Inverted.IndexPoint;

namespace Rsse.Tests.SearchEngine.Units;

[TestClass]
// тесты на оптимизированный элемент индекса
// нейминг типов или методов может измениться и разойтись с неймингом тестов
public class IndexPointTests
{
    [TestMethod]
    public void IndexPoint_ShouldCreate_EmptyHashMap()
    {
        Dictionary<int, int[]?> source = new();
        const int externalId = 777;
        const int externalCount = 333;
        const DictionaryStorageType searchType =
            DictionaryStorageType.HashTableStorage;

        var indexPoint = new IndexPoint(source, externalId, externalCount, searchType);

        Assert.AreEqual(0, indexPoint.Count);
        Assert.AreEqual(externalId, indexPoint.ExternalId);
        Assert.AreEqual(externalCount, indexPoint.ExternalCount);
        Assert.AreEqual(10, indexPoint.DataSize);
        Assert.AreEqual(DictionaryStorageType.HashTableStorage, indexPoint.SearchType);

        const int key = 0;

        var result0 = indexPoint.ContainsKey(key);
        Assert.IsFalse(result0);

        var result1 = indexPoint.ContainsKeyLinearScan(key);
        Assert.IsFalse(result1);

        var result2 = indexPoint.ContainsKeyBinarySearch(key);
        Assert.IsFalse(result2);

        var position3 = -1;
        var result3 = indexPoint.TryFindNextPositionLinearScan(key, ref position3);
        Assert.IsFalse(result3);
        Assert.AreEqual(-1, position3);

        var position4 = -1;
        var result4 = indexPoint.TryFindNextPositionBinarySearch(key, ref position4);
        Assert.IsFalse(result4);
        Assert.AreEqual(-1, position4);

        var result5 = indexPoint.TryFindTokenCountLinearScan(key, out var count5);
        Assert.IsFalse(result5);
        Assert.AreEqual(0, count5);

        var result6 = indexPoint.TryFindTokenCountBinarySearch(key, out var count6);
        Assert.IsFalse(result6);
        Assert.AreEqual(0, count6);

        var result7 = indexPoint.TryGetValue(key, out ReadOnlySpan<int> spanResult7);
        Assert.IsFalse(result7);
        Assert.AreEqual(0, spanResult7.Length);
    }

    [TestMethod]
    public void IndexPoint_ShouldCreate_EmptySortedArray()
    {
        Dictionary<int, int[]?> source = new();
        const int externalId = 777;
        const int externalCount = 333;
        const DictionaryStorageType searchType =
            DictionaryStorageType.SortedArrayStorage;

        var indexPoint = new IndexPoint(source, externalId, externalCount, searchType);

        Assert.AreEqual(0, indexPoint.Count);
        Assert.AreEqual(externalId, indexPoint.ExternalId);
        Assert.AreEqual(externalCount, indexPoint.ExternalCount);
        Assert.AreEqual(5, indexPoint.DataSize);
        Assert.AreEqual(DictionaryStorageType.SortedArrayStorage, indexPoint.SearchType);

        const int key = 0;

        var result0 = indexPoint.ContainsKey(key);
        Assert.IsFalse(result0);

        var result1 = indexPoint.ContainsKeyLinearScan(key);
        Assert.IsFalse(result1);

        var result2 = indexPoint.ContainsKeyBinarySearch(key);
        Assert.IsFalse(result2);

        var position3 = -1;
        var result3 = indexPoint.TryFindNextPositionLinearScan(key, ref position3);
        Assert.IsFalse(result3);
        Assert.AreEqual(-1, position3);

        var position4 = -1;
        var result4 = indexPoint.TryFindNextPositionBinarySearch(key, ref position4);
        Assert.IsFalse(result4);
        Assert.AreEqual(-1, position4);

        var result5 = indexPoint.TryFindTokenCountLinearScan(key, out var count5);
        Assert.IsFalse(result5);
        Assert.AreEqual(0, count5);

        var result6 = indexPoint.TryFindTokenCountBinarySearch(key, out var count6);
        Assert.IsFalse(result6);
        Assert.AreEqual(0, count6);

        var result7 = indexPoint.TryGetValue(key, out ReadOnlySpan<int> spanResult7);
        Assert.IsFalse(result7);
        Assert.AreEqual(0, spanResult7.Length);
    }

    [TestMethod]
    public void Enumerator_ShouldReturnAllKeyValuePairs()
    {
        // Arrange
        var source = new Dictionary<int, int[]?>
        {
            [1] = [10],
            [2] = [20, 30]
        };

        var indexPoint = new IndexPoint(
            source, 0, 0, DictionaryStorageType.HashTableStorage);

        var result = new Dictionary<int, int[]>();

        // Act
        foreach (var kvp in indexPoint)
        {
            result[kvp.Key] = kvp.Value;
        }

        // Assert
        Assert.HasCount(2, result);
        CollectionAssert.AreEqual(new[] { 10 }, result[1]);
        CollectionAssert.AreEqual(new[] { 20, 30 }, result[2]);
    }

    [TestMethod]
    public void TryFindNextPosition_ShouldFindNextPositionGreaterThanGiven()
    {
        // Arrange
        var source = new Dictionary<int, int[]?>
        {
            [1] = [5, 10, 15, 20]
        };

        var indexPoint = new IndexPoint(
            source, 0, 0, DictionaryStorageType.HashTableStorage);

        var position = 8;

        // Act
        var found = indexPoint.TryFindNextPositionBinarySearch(1, ref position);

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
        var indexPoint = new IndexPoint(
            emptySource, 0, 0, DictionaryStorageType.HashTableStorage);

        // Assert
        Assert.AreEqual(0, indexPoint.Count);
        Assert.IsEmpty(indexPoint.Keys);
        Assert.IsEmpty(indexPoint.Values);
    }

    private static readonly int[] ExpectedValues = [10, 20, 30];

    [TestMethod]
    public void Create_WithHashMapStorage_ShouldStoreAndRetrieveValues()
    {
        // Arrange
        var source = new Dictionary<int, int[]?>
        {
            [1] = [10, 20, 30],
            [2] = [40, 50],
            [3] = []
        };

        // Act
        var indexPoint = new IndexPoint(
            source,
            externalId: 100,
            externalCount: 5,
            DictionaryStorageType.HashTableStorage);

        // Assert
        Assert.AreEqual(3, indexPoint.Count);
        Assert.AreEqual(100, indexPoint.ExternalId);
        Assert.AreEqual(5, indexPoint.ExternalCount);
        Assert.AreEqual(DictionaryStorageType.HashTableStorage, indexPoint.SearchType);

        Assert.IsTrue(indexPoint.ContainsKey(1));
        Assert.IsTrue(indexPoint.TryGetValue(1, out int[] value1));
        CollectionAssert.AreEqual(ExpectedValues, value1);
    }

    [TestMethod]
    public void TryGetValue_WithNonExistentKey_ShouldReturnFalse()
    {
        // Arrange
        var source = new Dictionary<int, int[]?>
        {
            [1] = [10, 20]
        };

        var indexPoint = new IndexPoint(
            source, 0, 0, DictionaryStorageType.HashTableStorage);

        // Act & Assert
        Assert.IsFalse(indexPoint.ContainsKey(999));
        Assert.IsFalse(indexPoint.TryGetValue(999, out int[] _));
    }

    [TestMethod]
    public void Equality_Dictionary_ShouldBeEqualToItself()
    {
        // Arrange
        var source = new Dictionary<int, int[]?>
        {
            [1] = [10, 20]
        };

        // Act
        var indexPoint = new IndexPoint(source, 1, 1, DictionaryStorageType.HashTableStorage);

        // Assert
        Assert.AreEqual(indexPoint, indexPoint);
        // ReSharper disable once EqualExpressionComparison
        Assert.IsTrue(indexPoint == indexPoint);
    }

    private static readonly int[] ExpectedKeys = [3, 5, 7];

    [TestMethod]
    public void Create_WithSortedArrayStorageContainsEmpty_ShouldStoreAndRetrieveValues()
    {
        // Arrange
        var source = new Dictionary<int, int[]?>
        {
            [5] = [100, 200],
            [3] = [300],
            [7] = []
        };

        // Act
        var indexPoint = new IndexPoint(
            source,
            externalId: 200,
            externalCount: 3,
            DictionaryStorageType.SortedArrayStorage);

        // Assert
        Assert.AreEqual(3, indexPoint.Count);
        Assert.AreEqual(DictionaryStorageType.SortedArrayStorage, indexPoint.SearchType);

        // Keys should be accessible
        var keys = indexPoint.Keys.ToList();
        CollectionAssert.AreEqual(ExpectedKeys, keys.OrderBy(k => k).ToArray());
    }

    [TestMethod]
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
        var hashMapPoint = new IndexPoint(
            source, 0, 0, DictionaryStorageType.HashTableStorage);

        var sortedArrayPoint = new IndexPoint(
            source, 0, 0, DictionaryStorageType.SortedArrayStorage);

        // Assert - оба должны содержать одинаковые данные
        foreach (var key in source.Keys)
        {
            Assert.IsTrue(hashMapPoint.ContainsKey(key));
            Assert.IsTrue(sortedArrayPoint.ContainsKey(key));

            hashMapPoint.TryGetValue(key, out int[] hashMapValue);
            sortedArrayPoint.TryGetValue(key, out int[] sortedValue);

            CollectionAssert.AreEqual(hashMapValue, sortedValue);
        }
    }

    [TestMethod]
    public void SpecialValueHandling_ShortArrays_ShouldBeCompressed()
    {
        // Arrange
        var source = new Dictionary<int, int[]?>
        {
            [1] = [100], // Длина 1 элемент -> сжатие
            [2] = [200, 300], // Длина 2 элемента -> сжатие
            [3] = [400, 500, 600] // Длина больше 2х элементов -> обычное хранение
        };

        // Act
        var indexPoint = new IndexPoint(
            source, 0, 0, DictionaryStorageType.SortedArrayStorage);

        // Assert
        Assert.IsTrue(indexPoint.TryGetValue(1, out int[] value1));
        Assert.IsTrue(indexPoint.TryGetValue(2, out int[] value2));
        Assert.IsTrue(indexPoint.TryGetValue(3, out int[] value3));

        CollectionAssert.AreEqual(new[] { 100 }, value1);
        CollectionAssert.AreEqual(new[] { 200, 300 }, value2);
        CollectionAssert.AreEqual(new[] { 400, 500, 600 }, value3);
    }
}
