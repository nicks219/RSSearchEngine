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
    private readonly int _tagsCount = TestCatalogRepository.TagList.Count;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private ReadModel _readModel;
    private CustomProviderWithLogger<ReadModel> _customProviderWithLogger;
    private NoopLogger<ReadModel> _logger;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [TestInitialize]
    public void Initialize()
    {
        _customProviderWithLogger = new CustomProviderWithLogger<ReadModel>();
        _readModel = new ReadModel(_customProviderWithLogger.Scope);
        _logger = (NoopLogger<ReadModel>)_customProviderWithLogger.Provider.GetRequiredService<ILogger<ReadModel>>();
    }

    [TestMethod]
    public async Task ModelTagListTest_ShouldReports_ExpectedGenreCount()
    {
        // arrange & act:
        var response = await _readModel.ReadTagList();

        // assert:
        Assert.AreEqual(_tagsCount, response.StructuredTagsListResponse?.Count);
    }

    [TestMethod]
    public async Task ModelElectionTest_OnValidRequest_ShouldReturnNote()
    {
        // arrange:
        var request = new NoteDto { TagsCheckedRequest = [2] };

        // act:
        var response = await _readModel.GetNextOrSpecificNote(request, null, false);

        // assert:
        Assert.AreEqual(_logger.Message, string.Empty);
        Assert.AreEqual(TestCatalogRepository.FirstNoteText, response.TitleResponse);
    }

    [TestMethod]
    public async Task ModelElectionTest_OnInvalidRequest_ShouldReturnErrorMessageResponse()
    {
        // arrange:
        var request = new NoteDto { TagsCheckedRequest = [25000] };

        // act:
        var result = await _readModel.GetNextOrSpecificNote(request).ConfigureAwait(false);
        // ждём тестовый логер:
        var count = 20;
        while (count-- > 0 && !_logger.Reported)
        {
            await Task.Delay(100).ConfigureAwait(false);
        }

        // asserts:
        // todo: разберись - result нестабилен
        Assert.AreEqual(ModelMessages.ElectNoteError, result.CommonErrorMessageResponse);//
        Assert.AreEqual(ModelMessages.ElectNoteError, _logger.Message);
    }

    [TestMethod]
    public async Task ModelElectionTest_OnNullRequest_ShouldReturnEmptyResponse_ShouldNotLogError()
    {
        // arrange & act:
        var response = await _readModel.GetNextOrSpecificNote(null);

        // asserts:
        Assert.AreEqual(string.Empty, response.TitleResponse);
        Assert.AreEqual(string.Empty, _logger.Message);
    }

    [TestMethod]
    public async Task ControllerErrorLogTest_OnThrow_ShouldLogError()
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
    public async Task ControllerElectionTest_OnNullRequest_ShouldReturnEmptyTitle()
    {
        // arrange:
        var logger = Substitute.For<ILogger<ReadController>>();
        var factory = new CustomScopeFactory(_customProviderWithLogger.Provider);
        var readController = new ReadController(factory, logger);

        // act:
        var response = (await readController.GetNextOrSpecificNote(null, null)).Value;

        // assert:
        Assert.AreEqual(string.Empty, response?.TitleResponse);
    }

    [TestMethod]
    // демонстрация распределения результатов в текущем алгоритме выбора:
    public async Task DistributionTest_RandomHistogramViewer_ShouldHasGoodDistribution()
    {
        await using var repo = (TestCatalogRepository)_customProviderWithLogger.Provider.GetRequiredService<IDataRepository>();

        repo.CreateStubData(400);

        const double coefficient = 0.6D;

        const int notesCount = 389;

        var count = 100;

        var tempCount = count;

        var expectedNotesCount = Math.Min(notesCount, count) * coefficient;

        var request = new NoteDto { TagsCheckedRequest = new List<int>() };

        request.TagsCheckedRequest = Enumerable.Range(1, 44).ToList();

        var result = new Dictionary<int, int>();

        while (count-- > 0)
        {
            var response = await _readModel.GetNextOrSpecificNote(request);

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
