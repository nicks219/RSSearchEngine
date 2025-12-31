using System;
using System.Buffers;
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
    private readonly struct HashMap
    {
        public static int[] Create(Dictionary<int, int[]?> source, int externalId, int externalCount, DocumentDataPointSearchType searchType)
        {
            int[] data;

            var list = new List<KeyValuePair<int, int[]?>>(source);
            int count = list.Count;

            int[] hashCodes = list.Select(kv => kv.Key.GetHashCode()).ToArray();

            int bucketsCount = hashCodes.Length == 0
                ? 1
                : BucketHelper.ComputeOptimalBucketCount(hashCodes, true);

            ulong fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)bucketsCount);

            // Prepare bucket lists for chaining
            List<(int key, int[] value)>[] bucketLists = new List<(int key, int[] value)>[bucketsCount];
            for (int i = 0; i < bucketsCount; i++)
            {
                bucketLists[i] = new List<(int, int[])>();
            }

            // Distribute keys/values into buckets
            for (int i = 0; i < count; i++)
            {
                int key = list[i].Key;
                int[] value = list[i].Value ?? Array.Empty<int>();
                int hash = key.GetHashCode();
                int bucket = (int)HashHelpers.FastMod((uint)hash, (uint)bucketsCount, fastModMultiplier);
                bucketLists[bucket].Add((key, value));
            }

            // Sort keys within each bucket for binary search
            for (int i = 0; i < bucketsCount; i++)
            {
                bucketLists[i].Sort((a, b) => a.key.CompareTo(b.key));
            }

            int valuesSize = 0;

            // Calculate total keys and values size
            foreach (var bucket in bucketLists)
            {
                foreach (var (_, value) in bucket)
                {
                    valuesSize += value.Length;
                }
            }

            int bucketsSize = HashMapHeader.GetBucketsSize(bucketsCount);
            int bucketsOffset = HashMapHeader.GetBucketsOffset();
            int keysOffset = HashMapHeader.GetKeysOffset(bucketsOffset, bucketsSize);
            int keysSize = HashMapHeader.GetKeysSize(count);
            int valuesOffset = HashMapHeader.GetValuesOffset(keysOffset, keysSize);

            int dataSize = HashMapHeader.CalculateDataSize(bucketsSize, keysSize, valuesSize);

            data = new int[dataSize];
            DataPointHeader.WriteDataSize(data, dataSize);
            DataPointHeader.WriteCount(data, count);
            DataPointHeader.WriteSearchType(data, searchType);

            HashMapHeader.WriteBucketsCount(data, bucketsCount);
            HashMapHeader.WriteFastModMultiplier(data, fastModMultiplier);

            // Write buckets: [start, end] (order matches bucket index)
            int bucketsWritePos = bucketsOffset;
            int keysWritePos = keysOffset;
            int valuesWritePos = valuesOffset;
            int keysBase = keysOffset;
            int bucketKeyIndex = 0;
            for (int bucketIndex = 0; bucketIndex < bucketsCount; bucketIndex++)
            {
                var bucket = bucketLists[bucketIndex];
                int start = keysBase + bucketKeyIndex * DataPointHeader.KeyEntrySize;
                int end = start + bucket.Count * DataPointHeader.KeyEntrySize;
                data[bucketsWritePos++] = start;
                data[bucketsWritePos++] = end;
                foreach (var (key, value) in bucket)
                {
                    data[keysWritePos++] = key;
                    data[keysWritePos++] = value.Length;
                    data[keysWritePos++] = valuesWritePos;
                    Array.Copy(value, 0, data, valuesWritePos, value.Length);
                    valuesWritePos += value.Length;
                }
                bucketKeyIndex += bucket.Count;
            }

            // Write externalCount and externalId
            DataPointHeader.WriteExternalId(data, externalId);
            DataPointHeader.WriteExternalCount(data, externalCount);

            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsKey(int[] data, int key)
        {
            ulong fastModMultiplier = HashMapHeader.ReadFastModMultiplier(data);
            int bucketsCount = HashMapHeader.ReadBucketsCount(data);

            int hash = key.GetHashCode();
            int bucket = (int)HashHelpers.FastMod((uint)hash, (uint)bucketsCount, fastModMultiplier);
            int bucketBase = HashMapHeader.GetBucketsOffset() + bucket * HashMapHeader.BucketEntrySize;
            int start = data[bucketBase];
            int end = data[bucketBase + 1];
            int bucketSize = (end - start) / DataPointHeader.KeyEntrySize;

            if (bucketSize > BucketBinarySearchThreshold)
            {
                // Binary search within bucket
                int left = start, right = end - DataPointHeader.KeyEntrySize;

                while (left <= right)
                {
                    int mid = left + (((right - left) / DataPointHeader.KeyEntrySize) / 2) * DataPointHeader.KeyEntrySize;
                    int midKey = data[mid];

                    if (midKey == key)
                    {
                        return true;
                    }

                    if (midKey < key)
                    {
                        left = mid + DataPointHeader.KeyEntrySize;
                    }
                    else
                    {
                        right = mid - DataPointHeader.KeyEntrySize;
                    }
                }

                return false;
            }
            else
            {
                // Linear scan within bucket
                for (int i = start; i < end; i += DataPointHeader.KeyEntrySize)
                {
                    if (data[i] == key)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue(int[] data, int key, out int valueLength, out int valueOffset)
        {
            ulong fastModMultiplier = HashMapHeader.ReadFastModMultiplier(data);
            int bucketsCount = HashMapHeader.ReadBucketsCount(data);

            int hash = key.GetHashCode();
            int bucket = (int)HashHelpers.FastMod((uint)hash, (uint)bucketsCount, fastModMultiplier);
            int bucketBase = HashMapHeader.GetBucketsOffset() + bucket * HashMapHeader.BucketEntrySize;
            int start = data[bucketBase];
            int end = data[bucketBase + 1];
            int bucketSize = (end - start) / DataPointHeader.KeyEntrySize;

            if (bucketSize > BucketBinarySearchThreshold)
            {
                // Binary search within bucket
                int left = start, right = end - DataPointHeader.KeyEntrySize;

                while (left <= right)
                {
                    int mid = left + (((right - left) / DataPointHeader.KeyEntrySize) / 2) * DataPointHeader.KeyEntrySize;
                    int midKey = data[mid];

                    if (midKey == key)
                    {
                        valueLength = data[mid + 1];
                        valueOffset = data[mid + 2];
                        return true;
                    }

                    if (midKey < key)
                    {
                        left = mid + DataPointHeader.KeyEntrySize;
                    }
                    else
                    {
                        right = mid - DataPointHeader.KeyEntrySize;
                    }
                }

                valueLength = 0;
                valueOffset = 0;
                return false;
            }
            else
            {
                // Linear scan within bucket
                for (int i = start; i < end; i += DataPointHeader.KeyEntrySize)
                {
                    if (data[i] == key)
                    {
                        valueLength = data[i + 1];
                        valueOffset = data[i + 2];
                        return true;
                    }
                }

                valueLength = 0;
                valueOffset = 0;
                return false;
            }
        }

        public static IEnumerable<int> GetKeys(int[] data)
        {
            int count = DataPointHeader.ReadCount(data);

            int bucketsCount = HashMapHeader.ReadBucketsCount(data);

            int bucketsOffset = HashMapHeader.GetBucketsOffset();
            int bucketsSize = HashMapHeader.GetBucketsSize(bucketsCount);
            int keysOffset = HashMapHeader.GetKeysOffset(bucketsOffset, bucketsSize);
            int keysSize = HashMapHeader.GetKeysSize(count);
            int keysEnd = HashMapHeader.GetValuesOffset(keysOffset, keysSize);

            for (int i = keysOffset; i < keysEnd; i += DataPointHeader.KeyEntrySize)
            {
                yield return data[i];
            }
        }

        public static IEnumerable<int[]> GetValues(int[] data)
        {
            int count = DataPointHeader.ReadCount(data);

            int bucketsCount = HashMapHeader.ReadBucketsCount(data);

            int bucketsOffset = HashMapHeader.GetBucketsOffset();
            int bucketsSize = HashMapHeader.GetBucketsSize(bucketsCount);
            int keysOffset = HashMapHeader.GetKeysOffset(bucketsOffset, bucketsSize);
            int keysSize = HashMapHeader.GetKeysSize(count);
            int keysEnd = HashMapHeader.GetValuesOffset(keysOffset, keysSize);

            for (int i = keysOffset; i < keysEnd; i += DataPointHeader.KeyEntrySize)
            {
                int valueLength = data[i + 1];
                int valueOffset = data[i + 2];
                var arr = new int[valueLength];
                Array.Copy(data, valueOffset, arr, 0, valueLength);
                yield return arr;
            }
        }

        public static IEnumerator<KeyValuePair<int, int[]>> GetEnumerator(int[] data)
        {
            int count = DataPointHeader.ReadCount(data);

            int bucketsCount = HashMapHeader.ReadBucketsCount(data);

            int bucketsOffset = HashMapHeader.GetBucketsOffset();
            int bucketsSize = HashMapHeader.GetBucketsSize(bucketsCount);
            int keysOffset = HashMapHeader.GetKeysOffset(bucketsOffset, bucketsSize);
            int keysSize = HashMapHeader.GetKeysSize(count);
            int keysEnd = HashMapHeader.GetValuesOffset(keysOffset, keysSize);

            for (int i = keysOffset; i < keysEnd; i += DataPointHeader.KeyEntrySize)
            {
                int key = data[i];
                int valueLength = data[i + 1];
                int valueOffset = data[i + 2];
                var arr = new int[valueLength];
                Array.Copy(data, valueOffset, arr, 0, valueLength);
                yield return new KeyValuePair<int, int[]>(key, arr);
            }
        }

        private static class HashMapHeader
        {
            private const int HeaderSize = 6;
            public const int BucketEntrySize = 2;

            public static void WriteBucketsCount(int[] data, int bucketsCount)
            {
                data[3] = bucketsCount;
            }

            public static int ReadBucketsCount(int[] data)
            {
                return data[3];
            }

            public static void WriteFastModMultiplier(int[] data, ulong fastModMultiplier)
            {
                // fastModMultiplierLow
                data[4] = (int)(fastModMultiplier & 0xFFFFFFFF);
                // fastModMultiplierHigh
                data[5] = (int)(fastModMultiplier >> 32);
            }

            public static ulong ReadFastModMultiplier(int[] data)
            {
                return ((ulong)data[5] << 32) | (uint)data[4];
            }

            public static int CalculateDataSize(int bucketsSize, int keysSize, int valuesSize)
            {
                return HeaderSize + bucketsSize + keysSize + valuesSize + DataPointHeader.ExternalDataSize;
            }

            public static int GetBucketsOffset()
            {
                return HeaderSize;
            }

            public static int GetBucketsSize(int bucketsCount)
            {
                return bucketsCount * BucketEntrySize;
            }

            public static int GetKeysOffset(int bucketsOffset, int bucketsSize)
            {
                return bucketsOffset + bucketsSize;
            }

            public static int GetKeysSize(int count)
            {
                return count * DataPointHeader.KeyEntrySize;
            }

            public static int GetValuesOffset(int keysOffset, int keysSize)
            {
                return keysOffset + keysSize;
            }
        }
    }
}

public static class BucketHelper
{
    public static int ComputeOptimalBucketCount(ReadOnlySpan<int> hashCodes, bool hashCodesAreUnique = false)
    {
        HashSet<int>? uniqueSet = null;
        int min = hashCodes.Length;
        if (!hashCodesAreUnique)
        {
            uniqueSet = new HashSet<int>(hashCodes.ToArray());
            min = uniqueSet.Count;
        }

        int[] primes = {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919, 1103,
            1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229,
            30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751, 225307, 270371, 324449,
            389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249,
            3471899, 4166287, 4999559, 5999471, 7199369
        };

        int minBuckets = min * 2;
        int primeIdx = 0;
        while (primeIdx < primes.Length && minBuckets > primes[primeIdx])
            primeIdx++;
        if (primeIdx >= primes.Length)
            return GetPrime(min);

        int maxBuckets = min * (min >= 1000 ? 3 : 16);
        int maxIdx = primeIdx;
        while (maxIdx < primes.Length && maxBuckets > primes[maxIdx])
            maxIdx++;
        if (maxIdx < primes.Length)
            maxBuckets = primes[maxIdx - 1];

        int[] seenBuckets = ArrayPool<int>.Shared.Rent(maxBuckets / 32 + 1);
        int bestBuckets = maxBuckets;
        int bestCollisions = min;
        for (int idx = primeIdx; idx < maxIdx; idx++)
        {
            int numBuckets = primes[idx];
            Array.Clear(seenBuckets, 0, Math.Min(numBuckets, seenBuckets.Length));
            int collisions = 0;

            bool IsBucketFirstVisit(int code)
            {
                uint bucket = (uint)code % (uint)numBuckets;
                int arrIdx = (int)(bucket / 32);
                int bit = 1 << (int)(bucket % 32);
                if ((seenBuckets[arrIdx] & bit) != 0)
                {
                    collisions++;
                    if (collisions >= bestCollisions)
                        return false;
                }
                else
                {
                    seenBuckets[arrIdx] |= bit;
                }
                return true;
            }

            if (uniqueSet != null && min != hashCodes.Length)
            {
                foreach (int code in uniqueSet)
                {
                    if (!IsBucketFirstVisit(code))
                        break;
                }
            }
            else
            {
                for (int i = 0; i < hashCodes.Length && IsBucketFirstVisit(hashCodes[i]); i++) { }
            }

            if (collisions < bestCollisions)
            {
                bestBuckets = numBuckets;
                if ((double)collisions / min <= 0.05)
                {
                    bestCollisions = collisions;
                    break;
                }
                bestCollisions = collisions;
            }
        }
        ArrayPool<int>.Shared.Return(seenBuckets);
        return bestBuckets;
    }

    public static int GetPrime(int min)
    {
        int[] primes = {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919, 1103,
            1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229,
            30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751, 225307, 270371, 324449,
            389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249,
            3471899, 4166287, 4999559, 5999471, 7199369
        };
        foreach (int prime in primes)
            if (prime >= min)
                return prime;
        for (int i = (min | 1); i < int.MaxValue; i += 2)
        {
            bool isPrime = true;
            int sqrt = (int)Math.Sqrt(i);
            for (int j = 3; j <= sqrt; j += 2)
            {
                if (i % j == 0)
                {
                    isPrime = false;
                    break;
                }
            }
            if (isPrime)
                return i;
        }
        return min;
    }
}

public static class HashHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetFastModMultiplier(uint divisor) =>
        ulong.MaxValue / divisor + 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint FastMod(uint value, uint divisor, ulong multiplier)
    {
        // .NET 7+ fastmod
        return (uint)(((((multiplier * value) >> 32) + 1) * divisor) >> 32);
    }
}
