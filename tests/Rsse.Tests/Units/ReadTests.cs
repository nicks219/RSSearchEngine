using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SearchEngine.Api.Controllers;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Service.ApiModels;
using SearchEngine.Services;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.Repo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class ReadTests
{
    public const int ElectionTestNotesCount = 400;
    public const int ElectionTestTagsCount = 44;
    public const int ElectionTestCheckedTag = 2;

    public required ReadService ReadService;
    public required ServiceProviderStub Stub;

    private readonly CancellationToken _token = CancellationToken.None;
    private readonly int _tagsCount = FakeCatalogRepository.TagList.Count;

    [TestInitialize]
    public void Initialize()
    {
        Stub = new ServiceProviderStub();
        var repo = Stub.Provider.GetRequiredService<IDataRepository>();

        ReadService = new ReadService(repo);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Stub.Dispose();
    }

    [TestMethod]
    public async Task ReadService_ShouldReports_ExpectedTagsCount()
    {
        // arrange & act:
        var noteResultDto = await ReadService.ReadEnrichedTagList(_token);

        // assert:
        Assert.AreEqual(_tagsCount, noteResultDto.EnrichedTags?.Count);
    }

    [TestMethod] // ---
    public async Task ReadService_ShouldThrow_OnInvalidElectionRequest()
    {
        // arrange:
        const int key = 25000;
        var noteRequestDto = new NoteRequestDto
        (
            CheckedTags: [key],
            Title: default,
            Text: default,
            NoteIdExchange: default
        );

        // act:
        var exception =
            await TestHelper.GetExpectedExceptionWithAsync<KeyNotFoundException>(() =>
                ReadService.GetNextOrSpecificNote(noteRequestDto));

        // asserts:
        exception.EnsureNotNull();
        exception.Message.Should().Be($"The given key '{key}' was not present in the dictionary.");
    }

    [TestMethod]
    public async Task ReadService_ShouldProduceEmptyResponse_OnNullElectionRequest()
    {
        // arrange & act:
        var noteResultDto = await ReadService.GetNextOrSpecificNote(null, ct: _token);

        // asserts:
        Assert.AreEqual(string.Empty, noteResultDto.Title);
    }

    [TestMethod]
    public async Task ReadController_ShouldReturnNextNote_OnValidElectionRequestWithTags()
    {
        // arrange:
        var noteRequest = new NoteRequest { CheckedTags = [ElectionTestCheckedTag] };
        var logger = Substitute.For<ILogger<ReadController>>();
        var readManager = Stub.Provider.GetRequiredService<ReadService>();
        var updateManager = Stub.Provider.GetRequiredService<UpdateService>();

        // act:
        var readController = new ReadController(readManager, updateManager, logger);
        var noteResponse = await readController.GetNextOrSpecificNote(noteRequest, null, ct: _token);

        // assert:
        Assert.AreEqual(FakeCatalogRepository.FirstNoteTitle, noteResponse.Value.EnsureNotNull().Title);
    }

    [TestMethod]
    public async Task ReadController_ShouldReturnEmptyNote_OnRequestWithoutTags()
    {
        // arrange:
        using var host = new ServiceProviderStub();
        var readManager = Stub.Provider.GetRequiredService<ReadService>();
        var updateManager = Stub.Provider.GetRequiredService<UpdateService>();
        var logger = Substitute.For<ILogger<ReadController>>();

        var readController = new ReadController(readManager, updateManager, logger);
        readController.AddHttpContext(host.Provider);

        // act:
        var noteResponse = (await readController.GetNextOrSpecificNote(null, null, ct: _token))
            .Value
            .EnsureNotNull();

        // assert:
        Assert.AreEqual(string.Empty, noteResponse.Title);
        Assert.AreEqual(string.Empty, noteResponse.Text);
    }

    [TestMethod]
    public async Task ReadService_Election_ShouldReturnNextNote_OnValidElectionRequest()
    {
        // arrange:
        var noteRequestDto = new NoteRequestDto
        (
            CheckedTags: [ElectionTestCheckedTag],
            Title: default,
            Text: default,
            NoteIdExchange: default
        );

        // act:
        var noteResultDto = await ReadService.GetNextOrSpecificNote(noteRequestDto, null, false);

        // assert:
        Assert.AreEqual(FakeCatalogRepository.FirstNoteText, noteResultDto.Text);
    }

    [TestMethod]
    // NB: в тч демонстрация распределения результатов алгоритме выбора
    public async Task ReadService_Election_ShouldHasExpectedResponsesDistribution_OnElectionRequests()
    {
        var repo = (FakeCatalogRepository)Stub.Provider.GetRequiredService<IDataRepository>();

        repo.CreateStubData(ElectionTestNotesCount, _token);

        const double expectedCoefficient = 0.7D;

        var requestCount = 100;

        var tempCount = requestCount;

        var expectedNotesCount = Math.Min(ElectionTestNotesCount, requestCount) * expectedCoefficient;

        var noteRequestDto = new NoteRequestDto
        (
            CheckedTags: [],
            Title: default,
            Text: default,
            NoteIdExchange: default
        );

        noteRequestDto = noteRequestDto with { CheckedTags = Enumerable.Range(1, ElectionTestTagsCount).ToList() };

        var idStorage = new Dictionary<int, int>();

        while (requestCount-- > 0)
        {
            var noteResultDto = await ReadService.GetNextOrSpecificNote(noteRequestDto);

            var id = noteResultDto.NoteIdExchange;

            if (!idStorage.TryAdd(id, 1))
            {
                idStorage[id] += 1;
            }
        }

        using var _ = new AssertionScope();

        const int buckets = 10;

        const int evaluatedBucket = 2;

        var bucket = new int[buckets];

        // assert:
        idStorage.Count
            .Should()
            .BeGreaterThan((int)expectedNotesCount);

        Console.WriteLine("[get different '{0}' ids from '{1}' notes | by '{2}' calls]", idStorage.Count, ElectionTestNotesCount, tempCount);
        Console.Write("[with ids >= '{0}' repeats] ", evaluatedBucket);

        foreach (var (key, value) in idStorage)
        {
            if (value >= evaluatedBucket)
            {
                Console.Write(key + " - ");
            }

            for (var i = 0; i < bucket.Length; i++)
            {
                if (value > i)
                {
                    bucket[i] += 1;
                }
            }
        }

        Console.WriteLine("\n[repeat histogram: 1 - {0}]", buckets);

        foreach (var i in bucket)
        {
            Console.Write(i + " ");
        }
    }
}
