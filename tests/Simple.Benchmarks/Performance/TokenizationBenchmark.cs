using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Rsse.Domain.Data.Dto;
using Rsse.Domain.Service.Mapping;
using Rsse.Tests.Common;
using SimpleEngine.Benchmarks.Common;
using SimpleEngine.Dto.Common;
using SimpleEngine.Dto.Inverted;
using SimpleEngine.Indexes;
using SimpleEngine.Service;

namespace SimpleEngine.Benchmarks.Performance;

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

    private readonly SearchEngineManager _searchEngineManager = new(true);

    private readonly GeneralDirectIndexLegacy _generalDirectIndexLegacy = new();

    private readonly InvertedIndexLegacy _invertedIndexLegacyExtended = new();
    private readonly InvertedIndexLegacy _invertedIndexLegacyReduced = new();

    private readonly CommonIndices _commonIndexExtended = new(IndexPoint.DictionaryStorageType.SortedArrayStorage);

    private readonly CommonIndices _commonIndexHsExtended = new(IndexPoint.DictionaryStorageType.HashTableStorage);

    private readonly CommonIndices _commonIndexReduced = new(IndexPoint.DictionaryStorageType.SortedArrayStorage);

    private readonly CommonIndices _commonIndexHsReduced = new(IndexPoint.DictionaryStorageType.HashTableStorage);

    public static List<IndexType> Parameters =>
    [
        // IndexType.GeneralDirectLegacy,
        IndexType.InvertedLegacy,
        IndexType.InvertedExtended,
        // IndexType.InvertedIndexHsExtended,
        // IndexType.InvertedIndexReduced,
        // IndexType.InvertedIndexHsReduced
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
            case IndexType.GeneralDirectLegacy:
                {
                    _generalDirectIndexLegacy.Clear();
                    break;
                }
            case IndexType.InvertedLegacy:
                {
                    _invertedIndexLegacyExtended.Clear();
                    _invertedIndexLegacyReduced.Clear();
                    break;
                }
            case IndexType.InvertedExtended:
                {
                    _commonIndexExtended.Clear();
                    break;
                }
            case IndexType.InvertedExtendedHs:
                {
                    _commonIndexHsExtended.Clear();
                    break;
                }
            case IndexType.InvertedReduced:
                {
                    _commonIndexReduced.Clear();
                    break;
                }
            case IndexType.InvertedReducedHs:
                {
                    _commonIndexHsReduced.Clear();
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
                case IndexType.GeneralDirectLegacy:
                    {
                        _generalDirectIndexLegacy.TryAdd(documentId, tokenLine);
                        break;
                    }
                case IndexType.InvertedLegacy:
                    {
                        _invertedIndexLegacyExtended.TryAddDocument(documentId, tokenLine.Extended);
                        break;
                    }
                case IndexType.InvertedExtended:
                    {
                        _commonIndexExtended.AddOrUpdateVector(documentId, tokenLine.Extended);
                        break;
                    }
                case IndexType.InvertedExtendedHs:
                    {
                        _commonIndexHsExtended.AddOrUpdateVector(documentId, tokenLine.Extended);
                        break;
                    }
                case IndexType.InvertedReduced:
                    {
                        _commonIndexReduced.AddOrUpdateVector(documentId, tokenLine.Reduced);
                        break;
                    }
                case IndexType.InvertedReducedHs:
                    {
                        _commonIndexHsReduced.AddOrUpdateVector(documentId, tokenLine.Reduced);
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(IndexType), IndexType, null);
                    }
            }
        }

        /*if (IndexType == IndexType.InvertedLegacy)
        {
            _invertedIndexLegacyExtended.Compact();
            Console.WriteLine($"Inverted legacy count: {_invertedIndexLegacyExtended.Count}");
            _invertedIndexLegacyReduced.Compact();
            Console.WriteLine($"Inverted legacy count: {_invertedIndexLegacyReduced.Count}");
        }*/
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
