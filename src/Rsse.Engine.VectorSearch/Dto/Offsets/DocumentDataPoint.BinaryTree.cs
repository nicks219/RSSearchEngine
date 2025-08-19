using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RsseEngine.Dto.Offsets;

public readonly partial struct DocumentDataPoint
{
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
    private readonly struct BinaryTree
    {
        public static int[] Create(Dictionary<int, int[]?> source, int externalId, int externalCount, DocumentDataPointSearchType searchType)
        {
            int[] data;

            List<KeyValuePair<int, int[]?>> list = new List<KeyValuePair<int, int[]?>>(source);
            int count = list.Count;

            // Keys sorted for binary search
            List<KeyValuePair<int, int[]?>> sortedList = list.OrderBy(kv => kv.Key).ToList();

            //int valuesSize = sortedList.Sum(kv => kv.Value?.Length ?? 0);
            int valuesSize = sortedList.Sum(kv => kv.Value?.Length > 2 ? kv.Value.Length : 0);

            int keysOffset = BinaryTreeHeader.GetKeysOffset();
            int keysSize = BinaryTreeHeader.GetKeysSize(count);

            int metaOffset = BinaryTreeHeader.GetMetaOffset(keysOffset, count);

            int valuesOffset = BinaryTreeHeader.GetValuesOffset(keysOffset, keysSize);

            int dataSize = BinaryTreeHeader.CalculateDataSize(keysSize, valuesSize);

            data = new int[dataSize];
            DataPointHeader.WriteDataSize(data, dataSize);
            DataPointHeader.WriteCount(data, count);
            DataPointHeader.WriteSearchType(data, searchType);

            // Write sorted keys and values
            int keysWritePos = keysOffset;
            int metaWritePos = metaOffset;
            int valuesWritePos = valuesOffset;

            for (int i = 0; i < count; i++)
            {
                // write key
                int key = sortedList[i].Key;
                data[keysWritePos++] = key;

                // write value
                int[] value = sortedList[i].Value ?? Array.Empty<int>();

                if (value.Length > 2)
                {
                    data[metaWritePos++] = value.Length;
                    data[metaWritePos++] = valuesWritePos;

                    Array.Copy(value, 0, data, valuesWritePos, value.Length);
                    valuesWritePos += value.Length;
                }
                else
                {
                    if (value.Length == 2)
                    {
                        data[metaWritePos++] = -value[0];
                        data[metaWritePos++] = -value[1];
                    }
                    else
                    {
                        data[metaWritePos++] = -value[0];
                        data[metaWritePos++] = 0;
                    }
                }
            }

            // Write externalCount and externalId
            DataPointHeader.WriteExternalId(data, externalId);
            DataPointHeader.WriteExternalCount(data, externalCount);

            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValueLinearScan(int[] data, int key, out int valueLength, out int valueOffset)
        {
            int count = DataPointHeader.ReadCount(data);

            int keysOffset = BinaryTreeHeader.GetKeysOffset();
            int metaOffset = BinaryTreeHeader.GetMetaOffset(keysOffset, count);

            int index = data.AsSpan(keysOffset, count).IndexOf(key);

            if (index != -1)
            {
                int idx = index;
                valueLength = data[metaOffset + idx * 2];
                valueOffset = data[metaOffset + idx * 2 + 1];
                return true;
            }

            valueLength = 0;
            valueOffset = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValueBinarySearch(int[] data, int key, out int valueLength, out int valueOffset)
        {
            int count = DataPointHeader.ReadCount(data);

            int keysOffset = BinaryTreeHeader.GetKeysOffset();
            int metaOffset = BinaryTreeHeader.GetMetaOffset(keysOffset, count);

            // Binary search in sorted keys
            int left = keysOffset;
            int right = metaOffset - 1;

            while (left <= right)
            {
                int mid = left + ((right - left) / 2);
                int midKey = data[mid];

                if (midKey == key)
                {
                    int idx = mid - keysOffset;
                    valueLength = data[metaOffset + idx * 2];
                    valueOffset = data[metaOffset + idx * 2 + 1];
                    return true;
                }

                if (midKey < key)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            valueLength = 0;
            valueOffset = 0;
            return false;
        }

        public static IEnumerable<int> GetKeys(int[] data)
        {
            int count = DataPointHeader.ReadCount(data);

            int keysOffset = BinaryTreeHeader.GetKeysOffset();
            int metaOffset = BinaryTreeHeader.GetMetaOffset(keysOffset, count);

            for (int i = keysOffset; i < metaOffset; i++)
            {
                yield return data[i];
            }
        }

        public static IEnumerable<int[]> GetValues(int[] data)
        {
            int count = DataPointHeader.ReadCount(data);

            int keysOffset = BinaryTreeHeader.GetKeysOffset();
            int metaOffset = BinaryTreeHeader.GetMetaOffset(keysOffset, count);

            for (int i = 0; i < count; i++)
            {
                int valueLength = data[metaOffset + i * 2];
                int valueOffset = data[metaOffset + i * 2 + 1];
                int[] arr = new int[valueLength];
                Array.Copy(data, valueOffset, arr, 0, valueLength);
                yield return arr;
            }
        }

        public static IEnumerator<KeyValuePair<int, int[]>> GetEnumerator(int[] data)
        {
            int count = DataPointHeader.ReadCount(data);

            int keysOffset = BinaryTreeHeader.GetKeysOffset();
            int metaOffset = BinaryTreeHeader.GetMetaOffset(keysOffset, count);

            for (int i = 0; i < count; i++)
            {
                int key = data[keysOffset + i];
                int valueLength = data[metaOffset + i * 2];
                int valueOffset = data[metaOffset + i * 2 + 1];
                int[] arr = new int[valueLength];
                Array.Copy(data, valueOffset, arr, 0, valueLength);
                yield return new KeyValuePair<int, int[]>(key, arr);
            }
        }

        private static class BinaryTreeHeader
        {
            private const int HeaderSize = 3;

            public static int CalculateDataSize(int keysSize, int valuesSize)
            {
                return HeaderSize + keysSize + valuesSize + DataPointHeader.ExternalDataSize;
            }

            public static int GetKeysOffset()
            {
                return HeaderSize;
            }

            public static int GetKeysSize(int count)
            {
                // keys + (valueLength, valuesOffset) meta
                return count + count * 2;
            }

            public static int GetMetaOffset(int keysOffset, int count)
            {
                return keysOffset + count;
            }

            public static int GetValuesOffset(int keysOffset, int keysSize)
            {
                return keysOffset + keysSize;
            }
        }
    }
}
