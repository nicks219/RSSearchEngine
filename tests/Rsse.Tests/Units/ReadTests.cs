using System;
using System.Collections.Generic;
using System.Linq;
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
using SearchEngine.Service.Configuration;
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
    public required ServiceProviderStub Host;
    public required NoopLogger<ReadService> Logger;

    private readonly int _tagsCount = FakeCatalogRepository.TagList.Count;

    [TestInitialize]
    public void Initialize()
    {
        Host = new ServiceProviderStub();
        var repo = Host.Provider.GetRequiredService<IDataRepository>();
        var managerLogger = Host.Provider.GetRequiredService<ILogger<ReadService>>();

        ReadService = new ReadService(repo, managerLogger);
        Logger = (NoopLogger<ReadService>)Host.Provider.GetRequiredService<ILogger<ReadService>>();
    }

    [TestMethod]
    public async Task ReadManager_ShouldReports_ExpectedTagsCount()
    {
        // arrange & act:
        var noteResultDto = await ReadService.ReadEnrichedTagList();

        // assert:
        Assert.AreEqual(_tagsCount, noteResultDto.EnrichedTags?.Count);
    }

    [TestMethod]
    public async Task ReadManager_ShouldReturnErrorMessage_OnInvalidElectionRequest()
    {
        // arrange:
        var noteRequestDto = new NoteRequestDto
        (
            CheckedTags: [25000],
            Title: default,
            Text: default,
            NoteIdExchange: default
        );

        // act:
        var noteResultDto = await ReadService.GetNextOrSpecificNote(noteRequestDto).ConfigureAwait(false);
        // ждём тестовый логер:
        var count = 20;
        while (count-- > 0 && !Logger.Reported)
        {
            await Task.Delay(100).ConfigureAwait(false);
        }

        // asserts:
        Assert.AreEqual(ServiceErrorMessages.ElectNoteError, noteResultDto.ErrorMessage);
        Assert.AreEqual(ServiceErrorMessages.ElectNoteError, Logger.Message);
    }

    [TestMethod]
    public async Task ReadManager_ShouldProduceEmptyLogAndResponse_OnNullElectionRequest()
    {
        // arrange & act:
        var noteResultDto = await ReadService.GetNextOrSpecificNote(null);

        // asserts:
        Assert.AreEqual(string.Empty, noteResultDto.Title);
        Assert.AreEqual(string.Empty, Logger.Message);
    }

    [TestMethod]
    public async Task ReadController_ShouldReturnNextNote_OnValidElectionRequestWithTags()
    {
        // arrange:
        var noteRequest = new NoteRequest { CheckedTags = [ElectionTestCheckedTag] };
        var logger = Substitute.For<ILogger<ReadController>>();
        var readManager = Host.Provider.GetRequiredService<ReadService>();
        var updateManager = Host.Provider.GetRequiredService<UpdateService>();

        // act:
        var readController = new ReadController(readManager, updateManager, logger);
        var noteResponse = await readController.GetNextOrSpecificNote(noteRequest, null);

        // assert:
        Assert.AreEqual(FakeCatalogRepository.FirstNoteTitle, noteResponse.Value.EnsureNotNull().Title);
    }

    [TestMethod]
    public async Task ReadController_ShouldReturnEmptyNote_OnRequestWithoutTags()
    {
        // arrange:
        var host = new ServiceProviderStub();
        var readManager = Host.Provider.GetRequiredService<ReadService>();
        var updateManager = Host.Provider.GetRequiredService<UpdateService>();
        var logger = Substitute.For<ILogger<ReadController>>();

        var readController = new ReadController(readManager, updateManager, logger);
        readController.AddHttpContext(host.Provider);

        // act:
        var noteResponse = (await readController.GetNextOrSpecificNote(null, null))
            .Value
            .EnsureNotNull();

        // assert:
        Assert.AreEqual(string.Empty, noteResponse.Title);
        Assert.AreEqual(string.Empty, noteResponse.Text);
    }

    [TestMethod]
    public async Task ReadManager_Election_ShouldReturnNextNote_OnValidElectionRequest()
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
        Assert.AreEqual(Logger.Message, string.Empty);
        Assert.AreEqual(FakeCatalogRepository.FirstNoteText, noteResultDto.Text);
    }

    [TestMethod]
    // NB: в тч демонстрация распределения результатов алгоритме выбора
    public async Task ReadManager_Election_ShouldHasExpectedResponsesDistribution_OnElectionRequests()
    {
        await using var repo = (FakeCatalogRepository)Host.Provider.GetRequiredService<IDataRepository>();

        repo.CreateStubData(ElectionTestNotesCount);

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
