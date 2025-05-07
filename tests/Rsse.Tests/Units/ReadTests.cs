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
using SearchEngine.Domain.ApiModels;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Managers;
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

    public required ReadManager ReadManager;
    public required ServiceProviderStub<ReadManager> Host;
    public required NoopLogger<ReadManager> Logger;

    private readonly int _tagsCount = FakeCatalogRepository.TagList.Count;

    [TestInitialize]
    public void Initialize()
    {
        Host = new ServiceProviderStub<ReadManager>();
        var repo = Host.Provider.GetRequiredService<IDataRepository>();
        var managerLogger = Host.Provider.GetRequiredService<ILogger<ReadManager>>();

        ReadManager = new ReadManager(repo, managerLogger);
        Logger = (NoopLogger<ReadManager>)Host.Provider.GetRequiredService<ILogger<ReadManager>>();
    }

    [TestMethod]
    public async Task ReadManager_ShouldReports_ExpectedTagsCount()
    {
        // arrange & act:
        var responseDto = await ReadManager.ReadTagList();

        // assert:
        Assert.AreEqual(_tagsCount, responseDto.StructuredTagsListResponse?.Count);
    }

    [TestMethod]
    public async Task ReadManager_ShouldReturnErrorMessage_OnInvalidElectionRequest()
    {
        // arrange:
        var requestDto = new NoteRequestDto { TagsCheckedRequest = [25000] };

        // act:
        var responseDto = await ReadManager.GetNextOrSpecificNote(requestDto).ConfigureAwait(false);
        // ждём тестовый логер:
        var count = 20;
        while (count-- > 0 && !Logger.Reported)
        {
            await Task.Delay(100).ConfigureAwait(false);
        }

        // asserts:
        Assert.AreEqual(ErrorMessages.ElectNoteError, responseDto.CommonErrorMessageResponse);
        Assert.AreEqual(ErrorMessages.ElectNoteError, Logger.Message);
    }

    [TestMethod]
    public async Task ReadManager_ShouldProduceEmptyLogAndResponse_OnNullElectionRequest()
    {
        // arrange & act:
        var responseDto = await ReadManager.GetNextOrSpecificNote(null);

        // asserts:
        Assert.AreEqual(string.Empty, responseDto.TitleResponse);
        Assert.AreEqual(string.Empty, Logger.Message);
    }

    [TestMethod]
    public async Task ReadController_ShouldReturnNextNote_OnValidElectionRequestWithTags()
    {
        // arrange:
        var requestDto = new NoteRequest { TagsCheckedRequest = [ElectionTestCheckedTag] };
        var loggerFactory = Substitute.For<ILoggerFactory>();
        var repo = Host.Provider.GetRequiredService<IDataRepository>();
        var managerLogger = Host.Provider.GetRequiredService<ILogger<ReadManager>>();

        // act:
        var readController = new ReadController(repo, loggerFactory);
        var responseDto = await readController.GetNextOrSpecificNote(requestDto, null);

        // assert:
        Assert.AreEqual(FakeCatalogRepository.FirstNoteTitle, responseDto.Value.EnsureNotNull().TitleResponse);
    }

    [TestMethod]
    public async Task ReadController_ShouldReturnEmptyNote_OnRequestWithoutTags()
    {
        // arrange:
        var host = new ServiceProviderStub<ReadManager>();
        var repo = Host.Provider.GetRequiredService<IDataRepository>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        var managerLogger = Substitute.For<ILogger<ReadManager>>();

        var readController = new ReadController(repo, loggerFactory);
        readController.AddHttpContext(host.Provider);

        // act:
        var responseDto = (await readController.GetNextOrSpecificNote(null, null))
            .Value
            .EnsureNotNull();

        // assert:
        Assert.AreEqual(string.Empty, responseDto.TitleResponse);
        Assert.AreEqual(string.Empty, responseDto.TextResponse);
    }

    [TestMethod]
    public async Task ReadManager_Election_ShouldReturnNextNote_OnValidElectionRequest()
    {
        // arrange:
        var requestDto = new NoteRequestDto { TagsCheckedRequest = [ElectionTestCheckedTag] };

        // act:
        var responseDto = await ReadManager.GetNextOrSpecificNote(requestDto, null, false);

        // assert:
        Assert.AreEqual(Logger.Message, string.Empty);
        Assert.AreEqual(FakeCatalogRepository.FirstNoteText, responseDto.TextResponse);
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

        var requestDto = new NoteRequestDto { TagsCheckedRequest = new List<int>() };

        requestDto.TagsCheckedRequest = Enumerable.Range(1, ElectionTestTagsCount).ToList();

        var idStorage = new Dictionary<int, int>();

        while (requestCount-- > 0)
        {
            var responseDto = await ReadManager.GetNextOrSpecificNote(requestDto);

            var id = responseDto.NoteIdExchange;

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
