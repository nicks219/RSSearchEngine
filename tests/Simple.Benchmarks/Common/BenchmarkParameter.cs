using System;

namespace SimpleEngine.Benchmarks.Common;

public record BenchmarkParameter<T>(T SearchType, bool Pool = false) where T : Enum
{
    public override string ToString()
    {
        //return $"{SearchType,-20} {(Pool ? " Pool" : "")}";
        return $"{(Pool ? "Pool" : ""),-4} {SearchType}";
    }
}
