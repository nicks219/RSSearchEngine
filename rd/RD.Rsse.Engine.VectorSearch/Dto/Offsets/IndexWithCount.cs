namespace RD.RsseEngine.Dto.Offsets;

public readonly struct IndexWithCount(int index, int count)
{
    public readonly int Index = index;

    public readonly int Count = count;

    public override string ToString()
    {
        return $"index {Index} count {Count}";
    }
}
