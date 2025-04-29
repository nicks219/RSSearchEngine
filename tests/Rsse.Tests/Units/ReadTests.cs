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
using SearchEngine.Models;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.DatabaseRepo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class ReadTests
{
    public required ReadManager ReadManager;
    public required CustomServiceProvider<ReadManager> CustomServiceProvider;
    public required NoopLogger<ReadManager> Logger;

    private readonly int _tagsCount = FakeCatalogRepository.TagList.Count;

    [TestInitialize]
    public void Initialize()
    {
        CustomServiceProvider = new CustomServiceProvider<ReadManager>();
        ReadManager = new ReadManager(CustomServiceProvider.Scope);
        Logger = (NoopLogger<ReadManager>)CustomServiceProvider.Provider.GetRequiredService<ILogger<ReadManager>>();
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
    public async Task ReadManager_ShouldReturnNextNote_OnValidElectionRequest()
    {
        // arrange:
        var requestDto = new NoteDto { TagsCheckedRequest = [2] };

        // act:
        var responseDto = await ReadManager.GetNextOrSpecificNote(requestDto, null, false);

        // assert:
        Assert.AreEqual(Logger.Message, string.Empty);
        Assert.AreEqual(FakeCatalogRepository.FirstNoteText, responseDto.TitleResponse);
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
        Assert.AreEqual(ModelMessages.ElectNoteError, responseDto.CommonErrorMessageResponse);
        Assert.AreEqual(ModelMessages.ElectNoteError, Logger.Message);
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
    public async Task ReadController_ShouldLogError_WhenThrow()
    {
        // arrange:
        var logger = Substitute.For<ILogger<ReadController>>();
        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        serviceScopeFactory
            .When(s => s.CreateScope())
            .Do(i => throw new Exception());

        // act:
        var readController = new ReadController(serviceScopeFactory, logger);
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
        var factory = new CustomScopeFactory(CustomServiceProvider.Provider);
        var readController = new ReadController(factory, logger);

        // act:
        var responseDto = (await readController.GetNextOrSpecificNote(null, null)).Value;

        // assert:
        Assert.AreEqual(string.Empty, responseDto?.TitleResponse);
    }

    [TestMethod]
    // NB: в тч демонстрация распределения результатов алгоритме выбора
    public async Task NoteElector_ShouldHasExpectedDistribution_OnManyCalls()
    {
        await using var repo = (FakeCatalogRepository)CustomServiceProvider.Provider.GetRequiredService<IDataRepository>();

        repo.CreateStubData(400);

        const double expectedCoefficient = 0.6D;

        const int notesCount = 389;

        var count = 100;

        var tempCount = count;

        var expectedNotesCount = Math.Min(notesCount, count) * expectedCoefficient;

        var request = new NoteDto { TagsCheckedRequest = new List<int>() };

        request.TagsCheckedRequest = Enumerable.Range(1, 44).ToList();

        var result = new Dictionary<int, int>();

        while (count-- > 0)
        {
            var response = await ReadManager.GetNextOrSpecificNote(request);

            var id = response.CommonNoteId;

            if (!result.TryAdd(id, 1))
            {
                result[id] += 1;
            }
        }

        using var _ = new AssertionScope();

        const int buckets = 10;

        const int max = 10;

        var bucket = new int[buckets];

        result.Count
            .Should()
            .BeGreaterThan((int)expectedNotesCount);

        Console.WriteLine("[get: {0} from: {1} by: {2} calls with songs > {3} repeats:]", result.Count, notesCount, tempCount, max);

        foreach (var (key, value) in result)
        {
            if (value >= max)
            {
                Console.Write(key + ".");
            }

            for (var i = 0; i < bucket.Length; i++)
            {
                if (value > i)
                {
                    bucket[i] += 1;
                }
            }
        }

        Console.WriteLine("\n[histogram: 1 - {0}]", buckets);

        foreach (var i in bucket)
        {
            Console.Write(i + " ");
        }
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }
}
