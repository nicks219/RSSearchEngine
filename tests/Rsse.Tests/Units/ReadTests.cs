using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SearchEngine.Api.Controllers;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Service.ApiModels;
using SearchEngine.Services;
using SearchEngine.Tests.Integration.FakeDb.Extensions;
using SearchEngine.Tests.Units.Infra;

namespace SearchEngine.Tests.Units;

[TestClass]
public class ReadTests
{
    public const int ElectionTestNotesCount = 400;
    public const int ElectionTestTagsCount = 44;
    public const int ElectionTestCheckedTag = 2;

    public required ReadService ReadService;
    public required ServiceProviderStub ServiceProviderStub;

    private readonly CancellationToken _token = CancellationToken.None;
    private readonly int _tagsCount = FakeCatalogRepository.TagList.Count;

    [TestInitialize]
    public void Initialize()
    {
        ServiceProviderStub = new ServiceProviderStub();
        var repo = ServiceProviderStub.Provider.GetRequiredService<IDataRepository>();

        ReadService = new ReadService(repo);
    }

    [TestCleanup]
    public void Cleanup()
    {
        ServiceProviderStub.Dispose();
    }

    [TestMethod]
    public async Task ReadService_ShouldReports_ExpectedTagsCount()
    {
        // arrange & act:
        var noteResultDto = await ReadService.ReadEnrichedTagList(_token);

        // assert:
        Assert.AreEqual(_tagsCount, noteResultDto.EnrichedTags?.Count);
    }

    [TestMethod]
    public async Task ReadService_ShouldNotThrow_OnInvalidElectionRequest()
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
            await TestHelper.GetExpectedExceptionWithAsync<Exception>(() =>
                ReadService.GetNextOrSpecificNote(noteRequestDto, cancellationToken: _token));

        // asserts:
        exception.Should().BeNull();
    }

    [TestMethod]
    public async Task ReadService_ShouldProduceEmptyResponse_OnNullElectionRequest()
    {
        // arrange & act:
        var noteResultDto = await ReadService.GetNextOrSpecificNote(null, cancellationToken: _token);

        // asserts:
        Assert.AreEqual(string.Empty, noteResultDto.Title);
    }

    [TestMethod]
    public async Task ReadController_ShouldReturnNextNote_OnValidElectionRequestWithTags()
    {
        // arrange:
        var noteRequest = new NoteRequest { CheckedTags = [ElectionTestCheckedTag] };
        var readManager = ServiceProviderStub.Provider.GetRequiredService<ReadService>();
        var updateManager = ServiceProviderStub.Provider.GetRequiredService<UpdateService>();

        // act:
        var readController = new ReadController(readManager, updateManager);
        var noteResponse = await readController.GetNextOrSpecificNote(noteRequest, null, cancellationToken: _token);
        var result = ((OkObjectResult)noteResponse.Result.EnsureNotNull()).Value as NoteResponse;

        // assert:
        Assert.AreEqual(FakeCatalogRepository.FirstNoteTitle, result.EnsureNotNull().Title);
    }

    [TestMethod]
    public async Task ReadController_ShouldReturnEmptyNote_OnRequestWithoutTags()
    {
        // arrange:
        using var host = new ServiceProviderStub();
        var readManager = ServiceProviderStub.Provider.GetRequiredService<ReadService>();
        var updateManager = ServiceProviderStub.Provider.GetRequiredService<UpdateService>();

        var readController = new ReadController(readManager, updateManager);
        readController.AddHttpContext(host.Provider);

        // act:
        const int stubListCount = 3;
        var noteResponse = await readController.GetNextOrSpecificNote(null, null, cancellationToken: _token);
        var result = ((OkObjectResult)noteResponse.Result.EnsureNotNull()).Value as NoteResponse;

        // assert:
        Assert.AreEqual(string.Empty, result.EnsureNotNull().Title);
        Assert.AreEqual(string.Empty, result.Text);
        result.StructuredTags.EnsureNotNull().Count.Should().Be(stubListCount);
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
        var noteResultDto = await ReadService.GetNextOrSpecificNote(noteRequestDto, null, false, _token);

        // assert:
        Assert.AreEqual(FakeCatalogRepository.FirstNoteText, noteResultDto.Text);
    }

    [TestMethod]
    // Для round robin можно просто оценить distinct на полученных идентификаторах.
    // Но тк будет добавлен алгоритм случайного выбора на стороне .NET, оценка распределения также потребуется.
    public async Task ElectionRequests_ShouldHasExpectedResponsesDistribution_WithRoundRobin()
    {
        var repo = (FakeCatalogRepository)ServiceProviderStub.Provider.GetRequiredService<IDataRepository>();

        repo.CreateStubData(ElectionTestNotesCount, _token);

        const double expectedCoefficient = 1D;

        var requestCount = ElectionTestNotesCount;

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
            var noteResultDto = await ReadService
                .GetNextOrSpecificNote(noteRequestDto, randomElectionEnabled: false, cancellationToken: _token);

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
            .BeGreaterOrEqualTo((int)expectedNotesCount);

        Console.WriteLine("[get different '{0}' ids from '{1}' notes | by '{2}' calls]", idStorage.Count, ElectionTestNotesCount, tempCount);
        Console.Write("[with greater or equals to '{0}' repeats] ", evaluatedBucket);

        var evaluatedCounter = 0;
        foreach (var (key, value) in idStorage)
        {
            if (value >= evaluatedBucket)
            {
                evaluatedCounter++;
            }

            for (var i = 0; i < bucket.Length; i++)
            {
                if (value > i)
                {
                    bucket[i] += 1;
                }
            }
        }

        Console.Write($"{evaluatedCounter} unique notes");
        Console.WriteLine("\n[repeat histogram: 1 - {0}]", buckets);

        foreach (var i in bucket)
        {
            Console.Write(i + " ");
        }
    }
}
