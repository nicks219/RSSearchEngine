using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RsseEngine.Dto.Inverted;

/// <summary>
/// Высокопроизводительный неизменяемый компактный словарь для целочисленных ключей и массивов значений.
/// Данные хранятся в едином массиве для минимизации аллокаций и улучшения локальности памяти.
/// Поддерживает два режима хранения: хэш-таблица и отсортированный массив.
/// Используется как оптимизированный элемент с документом для дополнительного индекса (id документа -> токены с их позициями).
/// </summary>
[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
[SuppressMessage("ReSharper", "ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator")]
public readonly partial struct IndexPoint : IReadOnlyDictionary<int, int[]>, IEquatable<IndexPoint>
{
    /// <summary>
    /// Режим хранения словаря.
    /// </summary>
    public enum DictionaryStorageType
    {
        HashTableStorage = 0,
        SortedArrayStorage = 1
    }

    private const int BucketBinarySearchThreshold = 6;

    private readonly int[] _data;

    public IndexPoint(
        Dictionary<int, int[]?> source,
        int externalId,
        int externalCount,
        DictionaryStorageType searchType)
    {
        int[] data = searchType switch
        {
            DictionaryStorageType.HashTableStorage => HashTableStorage.Create(source, externalId, externalCount, searchType),
            DictionaryStorageType.SortedArrayStorage => SortedArrayStorage.Create(source, externalId, externalCount, searchType),
            _ => throw new NotSupportedException($"Only {nameof(DictionaryStorageType.HashTableStorage)} and {nameof(DictionaryStorageType.SortedArrayStorage)} are supported.")
        };

        _data = data;
    }

    public int DataSize => DataPointHeader.ReadDataSize(_data);
    public int Count => DataPointHeader.ReadCount(_data);
    public int ExternalId => DataPointHeader.ReadExternalId(_data);
    public int ExternalCount => DataPointHeader.ReadExternalCount(_data);
    public DictionaryStorageType SearchType => DataPointHeader.ReadSearchType(_data);

    public IEnumerable<int> Keys
    {
        get
        {
            int[] data = _data;
            var searchType = DataPointHeader.ReadSearchType(data);

            return searchType switch
            {
                DictionaryStorageType.HashTableStorage => HashTableStorage.GetKeys(data),
                DictionaryStorageType.SortedArrayStorage => SortedArrayStorage.GetKeys(data),
                _ => throw new NotSupportedException($"Only {nameof(DictionaryStorageType.HashTableStorage)} and {nameof(DictionaryStorageType.SortedArrayStorage)} are supported.")
            };
        }
    }

    public IEnumerable<int[]> Values
    {
        get
        {
            int[] data = _data;
            var searchType = DataPointHeader.ReadSearchType(data);

            return searchType switch
            {
                DictionaryStorageType.HashTableStorage => HashTableStorage.GetValues(data),
                DictionaryStorageType.SortedArrayStorage => SortedArrayStorage.GetValues(data),
                _ => throw new NotSupportedException($"Only {nameof(DictionaryStorageType.HashTableStorage)} and {nameof(DictionaryStorageType.SortedArrayStorage)} are supported.")
            };
        }
    }

    public bool ContainsKey(int key) => ContainsKeyBinarySearch(key);

    private static readonly int[] EmptyArray = [];

    public bool TryGetValue(int key, out int[] value)
    {
        if (TryGetValue(key, out ReadOnlySpan<int> span))
        {
            value = span.ToArray();
            return true;
        }

        value = EmptyArray;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindTokenCountLinearScan(int key, out int count)
    {
        int[] data = _data;

        if (TryGetValueLinearScan(data, key, out int valueLength, out int valueOffset))
        {
            count = valueLength;
            return true;
        }

        count = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindTokenCountBinarySearch(int key, out int count)
    {
        int[] data = _data;

        if (TryGetValueBinarySearch(data, key, out int valueLength, out int valueOffset))
        {
            count = valueLength;
            return true;
        }

        count = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryGetValueLinearScan(int[] data, int key, out int valueLength, out int valueOffset)
    {
        var searchType = DataPointHeader.ReadSearchType(data);

        return searchType switch
        {
            DictionaryStorageType.HashTableStorage => HashTableStorage.TryGetValue(data, key,
                out valueLength, out valueOffset),
            DictionaryStorageType.SortedArrayStorage => SortedArrayStorage.TryGetValueLinearScan(data, key,
                out valueLength, out valueOffset),
            _ => throw new NotSupportedException($"Only {nameof(DictionaryStorageType.HashTableStorage)} and {nameof(DictionaryStorageType.SortedArrayStorage)} are supported.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryGetValueBinarySearch(int[] data, int key, out int valueLength, out int valueOffset)
    {
        var searchType = DataPointHeader.ReadSearchType(data);

        return searchType switch
        {
            DictionaryStorageType.HashTableStorage => HashTableStorage.TryGetValue(data, key,
                out valueLength, out valueOffset),
            DictionaryStorageType.SortedArrayStorage => SortedArrayStorage.TryGetValueBinarySearch(data, key,
                out valueLength, out valueOffset),
            _ => throw new NotSupportedException($"Only {nameof(DictionaryStorageType.HashTableStorage)} and {nameof(DictionaryStorageType.SortedArrayStorage)} are supported.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // при использовании этого метода сжатие массивов теряет смысл
    public bool TryGetValue(int key, out ReadOnlySpan<int> valueSpan)
    {
        int[] data = _data;

        if (TryGetValueBinarySearch(data, key, out int valueLength, out int valueOffset))
        {
            // поддерживаем сжатый массив из 2х элементов
            if (valueLength < 0 & valueOffset < 0)
            {
                valueSpan = new[] { -valueLength, -valueOffset }.AsSpan();
                return true;
            }

            // поддерживаем сжатый массив из одного элемента
            if (valueLength < 0)
            {
                valueSpan = new[] { -valueLength }.AsSpan();
                return true;
            }

            valueSpan = new ReadOnlySpan<int>(data, valueOffset, valueLength);
            return true;
        }

        valueSpan = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKeyLinearScan(int key)
    {
        int[] data = _data;

        var searchType = DataPointHeader.ReadSearchType(data);

        return searchType switch
        {
            DictionaryStorageType.HashTableStorage => HashTableStorage.ContainsKey(data, key),
            DictionaryStorageType.SortedArrayStorage => SortedArrayStorage.ContainsKeyLinearScan(data, key),
            _ => throw new NotSupportedException($"Only {nameof(DictionaryStorageType.HashTableStorage)} and {nameof(DictionaryStorageType.SortedArrayStorage)} are supported.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKeyBinarySearch(int key)
    {
        int[] data = _data;

        var searchType = DataPointHeader.ReadSearchType(data);

        return searchType switch
        {
            DictionaryStorageType.HashTableStorage => HashTableStorage.ContainsKey(data, key),
            DictionaryStorageType.SortedArrayStorage => SortedArrayStorage.ContainsKeyBinarySearch(data, key),
            _ => throw new NotSupportedException($"Only {nameof(DictionaryStorageType.HashTableStorage)} and {nameof(DictionaryStorageType.SortedArrayStorage)} are supported.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindNextPositionLinearScan(int key, ref int position)
    {
        int[] data = _data;

        return TryGetValueLinearScan(data, key, out int size, out int offsetIndex)
               && TryFindNextPosition(data, size, offsetIndex, ref position);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindNextPositionBinarySearch(int key, ref int position)
    {
        int[] data = _data;

        return TryGetValueBinarySearch(data, key, out int size, out int offsetIndex)
               && TryFindNextPosition(data, size, offsetIndex, ref position);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryFindNextPosition(int[] data, int size, int offsetIndex, ref int position)
    {
        if (size > 0)
        {
            var offsetsSpan = data
                .AsSpan()
                .Slice(offsetIndex, size);

            foreach (var offsetElement in offsetsSpan)
            {
                if (offsetElement > position)
                {
                    position = offsetElement;
                    return true;
                }
            }

            return false;
        }

        var offset = -size;

        if (offset > position)
        {
            position = offset;
            return true;
        }

        offset = offsetIndex;

        if (offset >= 0)
        {
            return false;
        }

        offset = -offset;

        if (offset <= position)
        {
            return false;
        }

        position = offset;
        return true;
    }

    public int[] this[int key]
    {
        get
        {
            return TryGetValue(key, out int[] value)
                ? value
                : throw new KeyNotFoundException();
        }
    }

    public IEnumerator<KeyValuePair<int, int[]>> GetEnumerator()
    {
        int[] data = _data;
        var searchType = DataPointHeader.ReadSearchType(data);

        return searchType switch
        {
            DictionaryStorageType.HashTableStorage => HashTableStorage.GetEnumerator(data),
            DictionaryStorageType.SortedArrayStorage => SortedArrayStorage.GetEnumerator(data),
            _ => throw new NotSupportedException($"Only {nameof(DictionaryStorageType.HashTableStorage)} and {nameof(DictionaryStorageType.SortedArrayStorage)} are supported.")
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // IEquatable impl
    public bool Equals(IndexPoint other)
    {
        return _data.Equals(other._data);
    }

    public override bool Equals(object? obj)
    {
        return obj is IndexPoint other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _data.GetHashCode();
    }

    public static bool operator ==(IndexPoint left, IndexPoint right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(IndexPoint left, IndexPoint right)
    {
        return !left.Equals(right);
    }

    private static class DataPointHeader
    {
        public const int KeyEntrySize = 3;
        public const int ExternalDataSize = 2;

        public static void WriteDataSize(int[] data, int dataSize)
        {
            data[0] = dataSize;
        }

        public static int ReadDataSize(int[] data)
        {
            return data[0];
        }

        public static void WriteCount(int[] data, int count)
        {
            data[1] = count;
        }

        public static int ReadCount(int[] data)
        {
            return data[1];
        }

        public static void WriteSearchType(int[] data, DictionaryStorageType searchType)
        {
            data[2] = (int)searchType;
        }

        public static DictionaryStorageType ReadSearchType(int[] data)
        {
            return (DictionaryStorageType)data[2];
        }

        public static void WriteExternalCount(int[] data, int externalCount)
        {
            data[^2] = externalCount;
        }

        public static int ReadExternalCount(int[] data)
        {
            return data[^2];
        }

        public static void WriteExternalId(int[] data, int externalId)
        {
            data[^1] = externalId;
        }

        public static int ReadExternalId(int[] data)
        {
            return data[^1];
        }
    }
}
