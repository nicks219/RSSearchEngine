using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SimpleEngine.Dto.Offsets;

public readonly struct OffsetInfo(int size, int offsetIndex)
{
    public static void CreateOffsetInfo(List<int> tokenOffsets, List<OffsetInfo> offsetInfos, List<int> offsets)
    {
        // Оптимизируем хранение позиций токенов - если позиций больще двух то храним в Offsets,
        // иначе храним позиции в OffsetInfos - первую позицию в Size вторую позицию OffsetIndex.
        // Позиции в OffsetInfos храним как отрицательные,
        // если Size отрицательный или ноль - то это первая позиция,
        // если OffsetIndex отрицательный - то это вторая позиция,
        // если Size больше ноля - позиции хранятся в Offsets.

        OffsetInfo offsetInfo;

        if (tokenOffsets.Count > 2)
        {
            var position = offsets.Count;

            offsets.AddRange(tokenOffsets);
            offsetInfo = new OffsetInfo(tokenOffsets.Count, position);
        }
        else if (tokenOffsets.Count == 2)
        {
            offsetInfo = new OffsetInfo(-tokenOffsets[0], -tokenOffsets[1]);
        }
        else
        {
            offsetInfo = new OffsetInfo(-tokenOffsets[0], 0);
        }

        offsetInfos.Add(offsetInfo);
    }

    /// <summary>
    /// Оптимизация хранения позиций описана в <see cref="OffsetInfo.CreateOffsetInfo"/>
    /// </summary>
    /// <param name="offsets"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool TryFindNextPosition(List<int> offsets, ref int position)
    {
        if (size > 0)
        {
            var offsetsSpan = CollectionsMarshal.AsSpan(offsets)
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

    public override string ToString()
    {
        return $"Index {offsetIndex} Size {size}";
    }
}
