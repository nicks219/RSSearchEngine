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
using RandomSongSearchEngine.Controllers;
using RandomSongSearchEngine.Data.Dto;
using RandomSongSearchEngine.Service.Models;
using RandomSongSearchEngine.Tests.Infrastructure;

namespace RandomSongSearchEngine.Tests;

[TestClass]
public class ReadTest
{
    private const int GenresCount = 44;

    private ReadModel? _readModel;

    [TestInitialize]
    public void Initialize()
    {
        FakeLoggerErrors.ExceptionMessage = "";

        FakeLoggerErrors.LogErrorMessage = "";

        var host = new TestHost<ReadModel>();

        _readModel = new ReadModel(host.ServiceScope);
    }

    [TestMethod]
    public async Task Model_ShouldReports44Genres()
    {
        var response = await _readModel!.ReadTagList();

        Assert.AreEqual(GenresCount, response.GenreListResponse?.Count);
    }

    [TestMethod]
    public async Task Model_ShouldReadRandomSong()
    {
        // интеграционные тесты следует проводить на тестовой бд в docker'е
        var request = new NoteDto {SongGenres = new List<int> {11}};

        var response = await _readModel!.ElectNote(request);

        Assert.AreEqual("test title", response.TitleResponse);
    }

    [TestMethod]
    public async Task ModelInvalidRequest_ShouldResponseEmptyTitle()
    {
        var frontRequest = new NoteDto {SongGenres = new List<int> {1000}};

        var result = await _readModel!.ElectNote(frontRequest);

        Assert.AreEqual("", result.TitleResponse);
    }

    [TestMethod]
    public async Task ModelNullRequest_ShouldLoggingErrorInsideModel()
    {
        _ = await _readModel!.ElectNote(null!);

        Assert.AreNotEqual("[IndexModel: OnPost Error]", FakeLoggerErrors.LogErrorMessage);
    }

    [TestMethod]
    public async Task ModelNullRequest_ShouldResponseEmptyTitleTest()
    {
        var response = await _readModel!.ElectNote(null!);

        Assert.AreEqual("", response.TitleResponse);
    }

    [TestMethod]
    public async Task ControllerThrowsException_ShouldLogError()
    {
        var mockLogger = Substitute.For<ILogger<ReadController>>();
        var fakeServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        fakeServiceScopeFactory.When(s => s.CreateScope()).Do(i => throw new Exception());

        var readController = new ReadController(fakeServiceScopeFactory, mockLogger);

        _ = await readController.ElectNote(null!, null!);

        mockLogger.Received().LogError(Arg.Any<Exception>(), "[ReadController: OnPost Error]");
    }

    [TestMethod]
    public async Task ControllerNullRequest_ShouldResponseEmptyTitle()
    {
        var logger = Substitute.For<ILogger<ReadController>>();
        var factory = new CustomServiceScopeFactory(new TestHost<ReadModel>().ServiceProvider);
        var readController = new ReadController(factory, logger);

        var response = (await readController.ElectNote(null!, null!)).Value;

        Assert.AreEqual("", response?.TitleResponse);
    }

    [TestMethod]
    public async Task RandomizerDistributionTest()
    {
        const double coefficient = 0.7D;

        const int songsCount = 389;
        
        var count = 100;

        var tempCount = count;

        var expectedSongsCount = Math.Min(songsCount, count) * coefficient;

        var request = new NoteDto {SongGenres = new List<int>()};

        request.SongGenres = Enumerable.Range(1, 44).ToList();

        var result = new Dictionary<int, int>();

        while (count-- > 0)
        {
            var host = new TestHost<ReadModel>();

            _readModel = new ReadModel(host.ServiceScope);

            var response = await _readModel!.ElectNote(request);

            var id = response.Id!;

            if (result.ContainsKey(id))
            {
                result[id] += 1;
            }
            else
            {
                result.Add(id, 1);
            }
        }

        using var _ = new AssertionScope();

        const int buckets = 10;

        const int max = 10;

        var bucket = new int[buckets];

        result.Count
            .Should()
            .BeGreaterThan((int) expectedSongsCount);

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