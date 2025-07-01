using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using RsseEngine.Benchmarks.Common;
using RsseEngine.Dto;
using RsseEngine.SearchType;
using RsseEngine.Service;
using static RsseEngine.Benchmarks.Constants;

namespace RsseEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на TokenizerServiceCore.
/// Производится поиск тестового запроса во всех документах.
/// </summary>
[MinColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]
public class QueryBenchmarkReduced : IBenchmarkRunner
{
    private TokenizerServiceCore _tokenizer = null!;

    public static List<ReducedSearchType> Parameters =>
        ((ReducedSearchType[])Enum.GetValuesAsUnderlyingType<ReducedSearchType>())
        .ToList();

    [ParamsSource(nameof(Parameters))]
    // ReSharper disable once UnassignedField.Global
    public ReducedSearchType SearchType;

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    [GlobalSetup]
    public async Task SetupAsync()
    {
        await InitializeTokenizer(SearchType);
    }

    [Benchmark]
    public Dictionary<DocumentId, double> QueryReduced()
    {
        return _tokenizer.ComputeComplianceIndexReduced(SearchQuery, CancellationToken.None);
    }

    /// <inheritdoc/>
    public void RunBenchmark()
    {
        QueryReduced();
    }

    /// <inheritdoc/>
    public Task Initialize() => InitializeTokenizer(TokenizerReducedSearchType);

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    private async Task InitializeTokenizer(ReducedSearchType reducedSearchType)
    {
        Console.WriteLine(
            $"[{nameof(QueryBenchmarkReduced)}] reduced[{reducedSearchType}] initializing..");

        _tokenizer = new TokenizerServiceCore(ExtendedSearchType.Legacy, reducedSearchType);

        Console.WriteLine(
            $"[{nameof(QueryBenchmarkReduced)}] reduced[{reducedSearchType}] initializing..");

        var dataProvider = new FileDataProvider();
        var result = await _tokenizer.InitializeAsync(dataProvider, CancellationToken.None);

        Console.WriteLine(
            $"[{nameof(QueryBenchmarkReduced)}] reduced[{reducedSearchType}] initialized '{result:N0}' vectors.");
    }
}
