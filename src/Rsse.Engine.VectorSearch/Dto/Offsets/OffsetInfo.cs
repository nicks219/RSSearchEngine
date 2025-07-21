namespace RsseEngine.Dto.Offsets;

public readonly struct OffsetInfo(int size, int offsetIndex)
{
    public readonly int Size = size;

    public readonly int OffsetIndex = offsetIndex;

    public override string ToString()
    {
        return $"Index {OffsetIndex} Size {Size}";
    }
}
