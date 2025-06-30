using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using SearchEngine.Benchmarks.Common;
using static SearchEngine.Benchmarks.Constants;

namespace SearchEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на Lucene.
/// Производится поиск тестового запроса во всех документах.
/// </summary>
[MinColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Alphabetical)]
public class LuceneBenchmark : IBenchmarkRunner
{
    [GlobalSetup]
    public static async Task SetupAsync()
    {
        await InitializeLucene();
    }

    [Benchmark]
    public void FindSentence()
    {
        var result = LuceneWrapper.Find(SearchQuery);

        if (result.Count == 0)
        {
            Console.WriteLine("[Lucene] empty result");
        }

        // Console.WriteLine($"[{nameof(BenchmarkLucene)}] found: {result.Count}");
    }

    /// <inheritdoc/>
    public void RunBenchmark()
    {
        FindSentence();
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeLucene();

    /// <summary>
    /// Инициализировать Lucene.
    /// </summary>
    private static async Task InitializeLucene()
    {
        Console.WriteLine($"[{nameof(LuceneBenchmark)}] initializing..");

        await LuceneWrapper.InitializeAsync();
    }
}
