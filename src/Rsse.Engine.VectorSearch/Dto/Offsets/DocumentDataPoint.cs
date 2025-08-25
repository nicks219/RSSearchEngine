using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RsseEngine.Dto.Offsets;

/// <summary>
/// High-performance, immutable dictionary for int keys and int[] values (variable length).
/// If searchType == 0, uses:
/// Layout: [dataSize, count, searchType, bucketsCount, fastModMultiplierLow, fastModMultiplierHigh, bucket0, ..., bucketN, keys..., values..., externalCount, externalId]
/// Each bucket: [start, end]
/// Keys: [key0, valueLength0, valuesOffset0, ...]
/// Values: [v0, v1, ..., vN]
/// If searchType == 1, uses:
/// Layout: [dataSize, count, searchType, keys..., values..., externalCount, externalId]
/// Keys: [key0, key1, ..., keyN, valueLength0, valuesOffset0, valueLength1, valuesOffset1, ..., valueLengthN, valuesOffsetN]
/// Values: [v0, v1, ..., vN]
/// Keys should be sorted for binary search, use binary search for search in TryGetValue in this case.
/// </summary>
[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
[SuppressMessage("ReSharper", "ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator")]
public readonly partial struct DocumentDataPoint : IReadOnlyDictionary<int, int[]>, IEquatable<DocumentDataPoint>
{
    public enum DocumentDataPointSearchType : int
    {
        HashMap = 0,
        BinaryTree = 1
    }

    private const int BucketBinarySearchThreshold = 6;

    private readonly int[] _data;

    public DocumentDataPoint(Dictionary<int, int[]?> source, int externalId, int externalCount, DocumentDataPointSearchType searchType)
    {
        int[] data = searchType switch
        {
            DocumentDataPointSearchType.HashMap => HashMap.Create(source, externalId, externalCount, searchType),
            DocumentDataPointSearchType.BinaryTree => BinaryTree.Create(source, externalId, externalCount, searchType),
            _ => throw new NotSupportedException("Only searchType 0 and 1 are supported.")
        };

        _data = data;
    }

    public int DataSize => DataPointHeader.ReadDataSize(_data);
    public int Count => DataPointHeader.ReadCount(_data);
    public int ExternalId => DataPointHeader.ReadExternalId(_data);
    public int ExternalCount => DataPointHeader.ReadExternalCount(_data);
    public DocumentDataPointSearchType SearchType => DataPointHeader.ReadSearchType(_data);

    public IEnumerable<int> Keys
    {
        get
        {
            int[] data = _data;
            DocumentDataPointSearchType searchType = DataPointHeader.ReadSearchType(data);

            return searchType switch
            {
                DocumentDataPointSearchType.HashMap => HashMap.GetKeys(data),
                DocumentDataPointSearchType.BinaryTree => BinaryTree.GetKeys(data),
                _ => throw new NotSupportedException("Only searchType 0 and 1 are supported.")
            };
        }
    }

    public IEnumerable<int[]> Values
    {
        get
        {
            int[] data = _data;
            DocumentDataPointSearchType searchType = DataPointHeader.ReadSearchType(data);

            return searchType switch
            {
                DocumentDataPointSearchType.HashMap => HashMap.GetValues(data),
                DocumentDataPointSearchType.BinaryTree => BinaryTree.GetValues(data),
                _ => throw new NotSupportedException("Only searchType 0 and 1 are supported.")
            };
        }
    }

    public bool ContainsKey(int key) => TryGetValue(key, out ReadOnlySpan<int> value);

    public bool TryGetValue(int key, out int[] value)
    {
        if (TryGetValue(key, out ReadOnlySpan<int> span))
        {
            value = span.ToArray();
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryGetValueLinearScan(int[] data, int key, out int valueLength, out int valueOffset)
    {
        DocumentDataPointSearchType searchType = DataPointHeader.ReadSearchType(data);

        return searchType switch
        {
            DocumentDataPointSearchType.HashMap => HashMap.TryGetValue(data, key,
                out valueLength, out valueOffset),
            DocumentDataPointSearchType.BinaryTree => BinaryTree.TryGetValueLinearScan(data, key,
                out valueLength, out valueOffset),
            _ => throw new NotSupportedException("Only searchType 0 and 1 are supported.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryGetValueBinarySearch(int[] data, int key, out int valueLength, out int valueOffset)
    {
        DocumentDataPointSearchType searchType = DataPointHeader.ReadSearchType(data);

        return searchType switch
        {
            DocumentDataPointSearchType.HashMap => HashMap.TryGetValue(data, key,
                out valueLength, out valueOffset),
            DocumentDataPointSearchType.BinaryTree => BinaryTree.TryGetValueBinarySearch(data, key,
                out valueLength, out valueOffset),
            _ => throw new NotSupportedException("Only searchType 0 and 1 are supported.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(int key, out ReadOnlySpan<int> valueSpan)
    {
        int[] data = _data;

        if (TryGetValueBinarySearch(data, key, out int valueLength, out int valueOffset))
        {
            valueSpan = new ReadOnlySpan<int>(data, valueOffset, valueLength);
            return true;
        }
        else
        {
            valueSpan = default;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKeyLinearScan(int key)
    {
        int[] data = _data;

        DocumentDataPointSearchType searchType = DataPointHeader.ReadSearchType(data);

        return searchType switch
        {
            DocumentDataPointSearchType.HashMap => HashMap.ContainsKey(data, key),
            DocumentDataPointSearchType.BinaryTree => BinaryTree.ContainsKeyLinearScan(data, key),
            _ => throw new NotSupportedException("Only searchType 0 and 1 are supported.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKeyBinarySearch(int key)
    {
        int[] data = _data;

        DocumentDataPointSearchType searchType = DataPointHeader.ReadSearchType(data);

        return searchType switch
        {
            DocumentDataPointSearchType.HashMap => HashMap.ContainsKey(data, key),
            DocumentDataPointSearchType.BinaryTree => BinaryTree.ContainsKeyBinarySearch(data, key),
            _ => throw new NotSupportedException("Only searchType 0 and 1 are supported.")
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
            var offsetsSpan = data.AsSpan()
                .Slice(offsetIndex, size);

            //*
            foreach (var offset in offsetsSpan)
            {
                if (offset > position)
                {
                    position = offset;
                    return true;
                }
            }
            return false;
            /*/
            var offset = offsetsSpan.BinarySearch(position + 1);

            if (offset < 0)
            {
                offset = ~offset;
                if (offset == offsetsSpan.Length)
                {
                    return false;
                }
            }

            position = offsetsSpan[offset];
            return true;
            //*/
        }
        else
        {
            var offset = -size;

            if (offset > position)
            {
                position = offset;
                return true;
            }

            offset = offsetIndex;

            if (offset < 0)
            {
                offset = -offset;

                if (offset > position)
                {
                    position = offset;
                    return true;
                }
            }

            return false;
        }
    }

    public int[] this[int key]
    {
        get
        {
            if (TryGetValue(key, out int[] value))
            {
                return value;
            }

            throw new KeyNotFoundException();
        }
    }

    public IEnumerator<KeyValuePair<int, int[]>> GetEnumerator()
    {
        int[] data = _data;
        DocumentDataPointSearchType searchType = DataPointHeader.ReadSearchType(data);

        return searchType switch
        {
            DocumentDataPointSearchType.HashMap => HashMap.GetEnumerator(data),
            DocumentDataPointSearchType.BinaryTree => BinaryTree.GetEnumerator(data),
            _ => throw new NotSupportedException("Only searchType 0 and 1 are supported.")
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(DocumentDataPoint other)
    {
        return _data.Equals(other._data);
    }

    public override bool Equals(object? obj)
    {
        return obj is DocumentDataPoint other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _data.GetHashCode();
    }

    public static bool operator ==(DocumentDataPoint left, DocumentDataPoint right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DocumentDataPoint left, DocumentDataPoint right)
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

        public static void WriteSearchType(int[] data, DocumentDataPointSearchType searchType)
        {
            data[2] = (int)searchType;
        }

        public static DocumentDataPointSearchType ReadSearchType(int[] data)
        {
            return (DocumentDataPointSearchType)data[2];
        }

        public static void WriteExternalCount(int[] data, int externalCount)
        {
            data[data.Length - 2] = externalCount;
        }

        public static int ReadExternalCount(int[] data)
        {
            return data[data.Length - 2];
        }

        public static void WriteExternalId(int[] data, int externalId)
        {
            data[data.Length - 1] = externalId;
        }

        public static int ReadExternalId(int[] data)
        {
            return data[data.Length - 1];
        }
    }
}
