using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using SearchEngine.Benchmarks.Common;
using static SearchEngine.Benchmarks.Constants;

namespace SearchEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на Lucene.
/// </summary>
[MinColumn]
public class LuceneBenchmark : IBenchmarkRunner
{
    private static bool _isInitialized;

    [GlobalSetup]
    public static async Task SetupAsync()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        Console.WriteLine($"[{nameof(LuceneBenchmark)}] initializing..");

        await InitializeLucene();
    }

    /// <inheritdoc/>
    [Benchmark]
    public void RunBenchmark()
    {
        var result = LuceneWrapper.Find(SearchQuery);

        if (result.Count == 0)
        {
            Console.WriteLine("[Lucene] empty result");
        }

        // Console.WriteLine($"[{nameof(BenchmarkLucene)}] found: {result.Count}");
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeLucene();

    /// <summary>
    /// Инициализировать Lucene.
    /// </summary>
    private static async Task InitializeLucene()
    {
        Console.WriteLine($"[{nameof(LuceneWrapper)}] initializing..");
        await LuceneWrapper.InitializeAsync();
    }
}
