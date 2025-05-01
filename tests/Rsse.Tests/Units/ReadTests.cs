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
using SearchEngine.Common;
using SearchEngine.Controllers;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Managers;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Integrations.Infra;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.DatabaseRepo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class ReadTests
{
    public const int ElectionTestNotesCount = 400;
    public const int ElectionTestTagsCount = 44;
    public const int ElectionTestCheckedTag = 2;

    public required ReadManager ReadManager;
    public required ServicesStubStartup<ReadManager> Host;
    public required NoopLogger<ReadManager> Logger;

    private readonly int _tagsCount = FakeCatalogRepository.TagList.Count;

    [TestInitialize]
    public void Initialize()
    {
        Host = new ServicesStubStartup<ReadManager>();
        ReadManager = new ReadManager(Host.Provider);
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
        var requestDto = new NoteDto { TagsCheckedRequest = [25000] };

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
    public async Task ReadController_ShouldLogError_OnUndefinedRequest()
    {
        // arrange:
        var logger = Substitute.For<ILogger<ReadController>>();
        // var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        // serviceScopeFactory.When(s => s.CreateScope()).Do(i => throw new Exception());

        // act:
        var readController = new ReadController(logger);
        _ = await readController.GetNextOrSpecificNote(null, null);

        // assert:
        logger
            .Received()
            .LogError(Arg.Any<Exception>(), ControllerMessages.ElectNoteError);
    }

    [TestMethod]
    public async Task ReadController_ShouldReturnEmptyTitle_OnNullElectionRequest()
    {
        // arrange:
        var logger = Substitute.For<ILogger<ReadController>>();
        var host = new ServicesStubStartup<ReadManager>();
        var readController = new ReadController(logger);
        readController.AddHttpContext(host.Provider);

        // act:
        var responseDto = (await readController.GetNextOrSpecificNote(null, null)).Value;

        // assert:
        Assert.AreEqual(string.Empty, responseDto?.TitleResponse);
    }

    [TestMethod]
    public async Task ReadManager_Election_ShouldReturnNextNote_OnValidElectionRequest()
    {
        // arrange:
        var requestDto = new NoteDto { TagsCheckedRequest = [ElectionTestCheckedTag] };

        // act:
        var responseDto = await ReadManager.GetNextOrSpecificNote(requestDto, null, false);

        // assert:
        Assert.AreEqual(Logger.Message, string.Empty);
        Assert.AreEqual(FakeCatalogRepository.FirstNoteText, responseDto.TitleResponse);
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

        var requestDto = new NoteDto { TagsCheckedRequest = new List<int>() };

        requestDto.TagsCheckedRequest = Enumerable.Range(1, ElectionTestTagsCount).ToList();

        var idStorage = new Dictionary<int, int>();

        while (requestCount-- > 0)
        {
            var responseDto = await ReadManager.GetNextOrSpecificNote(requestDto);

            var id = responseDto.CommonNoteId;

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
