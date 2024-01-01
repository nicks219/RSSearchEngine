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
    private TestServiceCollection<ReadModel> _serviceCollection;
    private TestLogger<ReadModel> _logger;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [TestInitialize]
    public void Initialize()
    {
        _serviceCollection = new TestServiceCollection<ReadModel>();
        _readModel = new ReadModel(_serviceCollection.Scope);
        _logger = (TestLogger<ReadModel>)_serviceCollection.Provider.GetRequiredService<ILogger<ReadModel>>();
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
        var request = new NoteDto { TagsCheckedRequest = new List<int> { 2 } };

        // act:
        var response = await _readModel.GetNextOrSpecificNote(request, null, false);

        // assert:
        Assert.AreEqual(_logger.ErrorMessage, string.Empty);
        Assert.AreEqual(TestCatalogRepository.FirstNoteText, response.TitleResponse);
    }

    [TestMethod]
    public async Task ModelElectionTest_OnInvalidRequest_ShouldReturnErrorMessageResponse()
    {
        // arrange:
        var request = new NoteDto { TagsCheckedRequest = new List<int> { 2500 } };

        // act:
        var result = await _readModel.GetNextOrSpecificNote(request);
        // ждём тестовый логер:
        await Task.Delay(100);

        // asserts:
        Assert.AreEqual(ReadModel.ElectNoteError, _logger.ErrorMessage);
        Assert.AreEqual(ReadModel.ElectNoteError, result.CommonErrorMessageResponse);
    }

    [TestMethod]
    public async Task ModelElectionTest_OnNullRequest_ShouldReturnEmptyResponse_ShouldNotLogError()
    {
        // arrange & act:
        var response = await _readModel.GetNextOrSpecificNote(null);

        // asserts:
        Assert.AreEqual(string.Empty, response.TitleResponse);
        Assert.AreEqual(string.Empty, _logger.ErrorMessage);
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
            .LogError(Arg.Any<Exception>(), ReadController.ElectNoteError);
    }

    [TestMethod]
    public async Task ControllerElectionTest_OnNullRequest_ShouldReturnEmptyTitle()
    {
        // arrange:
        var logger = Substitute.For<ILogger<ReadController>>();
        var factory = new TestServiceScopeFactory(_serviceCollection.Provider);
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
        var __ = _serviceCollection.Provider.GetRequiredService<IDataRepository>();
        TestCatalogRepository.CreateStubData(400);

        // TODO: сделать метод, добавляющий N случайных заметок для проведения теста
        const double coefficient = 0.7D;

        const int songsCount = 389;

        var count = 100;

        var tempCount = count;

        var expectedSongsCount = Math.Min(songsCount, count) * coefficient;

        var request = new NoteDto { TagsCheckedRequest = new List<int>() };

        request.TagsCheckedRequest = Enumerable.Range(1, 44).ToList();

        var result = new Dictionary<int, int>();

        while (count-- > 0)
        {
            var host = new TestServiceCollection<ReadModel>();

            _readModel = new ReadModel(host.Scope);

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
            .BeGreaterThan((int)expectedSongsCount);

        Console.WriteLine("[get: {0} from: {1} by: {2} calls with songs > {3} repeats:]", result.Count, songsCount, tempCount, max);

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
