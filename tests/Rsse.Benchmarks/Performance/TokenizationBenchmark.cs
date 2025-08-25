using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Rsse.Domain.Data.Dto;
using Rsse.Domain.Service.Mapping;
using Rsse.Tests.Common;
using RsseEngine.Benchmarks.Common;
using RsseEngine.Dto;
using RsseEngine.Dto.Offsets;
using RsseEngine.Indexes;
using RsseEngine.Service;

namespace RsseEngine.Benchmarks.Performance;

/// <summary>
/// Инициализация и бенчмарк на TokenizerServiceCore.
/// Производится поиск дубликатов во всех документах.
/// </summary>
[MinColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]
public class TokenizationBenchmark
{
    private readonly FileDataMultipleProvider _fileDataMultipleProvider = new(100);

    private readonly SearchEngineManager _searchEngineManager = new(true, true);

    private readonly DirectIndex _generalDirectIndex = new();

    private readonly InvertedOffsetIndex _invertedOffsetIndexExtended = new();

    private readonly ArrayDirectOffsetIndex _arrayDirectOffsetIndexExtended = new(DocumentDataPoint.DocumentDataPointSearchType.BinaryTree);

    private readonly ArrayDirectOffsetIndex _arrayDirectOffsetIndexHsExtended = new(DocumentDataPoint.DocumentDataPointSearchType.HashMap);

    private readonly InvertedIndex<DocumentIdList> _invertedIndexReduced = new();

    private readonly ArrayDirectOffsetIndex _arrayDirectOffsetIndexReduced = new(DocumentDataPoint.DocumentDataPointSearchType.BinaryTree);

    private readonly ArrayDirectOffsetIndex _arrayDirectOffsetIndexHsReduced = new(DocumentDataPoint.DocumentDataPointSearchType.HashMap);

    public static List<IndexType> Parameters =>
    [
        IndexType.GeneralDirect,
        IndexType.InvertedOffsetIndexExtended,
        IndexType.ArrayDirectOffsetIndexExtended,
        IndexType.ArrayDirectOffsetIndexHsExtended,
        IndexType.InvertedIndexReduced,
        IndexType.ArrayDirectOffsetIndexReduced,
        IndexType.ArrayDirectOffsetIndexHsReduced
    ];

    [ParamsSource(nameof(Parameters))]
    // ReSharper disable once UnassignedField.Global
    public required IndexType IndexType;

    /// <summary>
    /// Инициализировать RSSE токенайзер.
    /// </summary>
    [GlobalSetup]
    public async Task SetupAsync()
    {
        Console.WriteLine($"FileDataMultipleProvider {IndexType} initializing..");

        await _fileDataMultipleProvider.Initialize();

        Console.WriteLine($"FileDataMultipleProvider {IndexType} initialized..");
    }

    [Benchmark]
    public async Task InitializeTokenizer()
    {
        switch (IndexType)
        {
            case IndexType.GeneralDirect:
                {
                    _generalDirectIndex.Clear();
                    break;
                }
            case IndexType.InvertedOffsetIndexExtended:
                {
                    _invertedOffsetIndexExtended.Clear();
                    break;
                }
            case IndexType.ArrayDirectOffsetIndexExtended:
                {
                    _arrayDirectOffsetIndexExtended.Clear();
                    break;
                }
            case IndexType.ArrayDirectOffsetIndexHsExtended:
                {
                    _arrayDirectOffsetIndexHsExtended.Clear();
                    break;
                }
            case IndexType.InvertedIndexReduced:
                {
                    _invertedIndexReduced.Clear();
                    break;
                }
            case IndexType.ArrayDirectOffsetIndexReduced:
                {
                    _arrayDirectOffsetIndexReduced.Clear();
                    break;
                }
            case IndexType.ArrayDirectOffsetIndexHsReduced:
                {
                    _arrayDirectOffsetIndexHsReduced.Clear();
                    break;
                }
            default:
                {
                    throw new ArgumentOutOfRangeException(nameof(IndexType), IndexType, null);
                }
        }

        var notes = _fileDataMultipleProvider.GetDataAsync();

        await foreach (var note in notes)
        {
            var requestNote = note.MapToDto();
            var tokenLine = CreateTokensLine(requestNote);
            var documentId = new DocumentId(note.NoteId);

            switch (IndexType)
            {
                case IndexType.GeneralDirect:
                    {
                        _generalDirectIndex.TryAdd(documentId, tokenLine);
                        break;
                    }
                case IndexType.InvertedOffsetIndexExtended:
                    {
                        _invertedOffsetIndexExtended.AddOrUpdateVector(documentId, tokenLine.Extended);
                        break;
                    }
                case IndexType.ArrayDirectOffsetIndexExtended:
                    {
                        _arrayDirectOffsetIndexExtended.AddOrUpdateVector(documentId, tokenLine.Extended);
                        break;
                    }
                case IndexType.ArrayDirectOffsetIndexHsExtended:
                    {
                        _arrayDirectOffsetIndexHsExtended.AddOrUpdateVector(documentId, tokenLine.Extended);
                        break;
                    }
                case IndexType.InvertedIndexReduced:
                    {
                        _invertedIndexReduced.AddVector(documentId, tokenLine.Reduced);
                        break;
                    }
                case IndexType.ArrayDirectOffsetIndexReduced:
                    {
                        _arrayDirectOffsetIndexReduced.AddOrUpdateVector(documentId, tokenLine.Reduced);
                        break;
                    }
                case IndexType.ArrayDirectOffsetIndexHsReduced:
                    {
                        _arrayDirectOffsetIndexHsReduced.AddOrUpdateVector(documentId, tokenLine.Reduced);
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(IndexType), IndexType, null);
                    }
            }
        }
    }

    private TokenLine CreateTokensLine(TextRequestDto note)
    {
        if (note.Text == null || note.Title == null)
            throw new ArgumentNullException(nameof(note), "Request text or title should not be null.");

        var extendedTokenLine = _searchEngineManager.TokenizeTextExtended(note.Text, " ", note.Title);

        var reducedTokenLine = _searchEngineManager.TokenizeTextReduced(note.Text, " ", note.Title);

        return new TokenLine(Extended: extendedTokenLine, Reduced: reducedTokenLine);
    }
}
