# if IS_RD_PROJECT

namespace SimpleEngine.Dto.Offsets;

public readonly struct IndexWithLast(int index, bool last)
{
    public readonly int Index = index;

    public readonly bool Last = last;

    public override string ToString()
    {
        return $"Index {Index} Last {Last}";
    }
}


#endif
