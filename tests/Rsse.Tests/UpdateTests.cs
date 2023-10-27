using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Data.Dto;
using SearchEngine.Infrastructure.Cache;
using SearchEngine.Service.Models;
using SearchEngine.Tests.Infrastructure;
using SearchEngine.Tests.Infrastructure.DAL;

namespace SearchEngine.Tests;

[TestClass]
public class UpdateTests
{
    private const string TestName = "0: key";
    private const string TestText = "test text text";
    private int _testSongId;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private UpdateModel _updateModel;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [TestInitialize]
    public void Initialize()
    {
        var hostCacheTyped = new TestServiceCollection<CacheRepository>();
        var hostModelTyped = new TestServiceCollection<UpdateModel>();
        var findModel = new FindModel(hostCacheTyped.Scope);

        TestDataRepository.CreateStubData(10);
        _testSongId = findModel.FindIdByName(TestName);
        _updateModel = new UpdateModel(hostModelTyped.Scope);
    }

    [TestMethod]
    public async Task ModelTagList_ShouldReports_ExpectedGenreCount()
    {
        // arrange & act:
        var response = await _updateModel.GetOriginalNote(1);

        // assert:
        Assert.AreEqual(TestDataRepository.TagList.Count, response.GenreListResponse?.Count);
    }

    [TestMethod]
    public async Task ModelUpdateTest_ShouldUpdateNoteToExpected()
    {
        // arrange:
        var song = new NoteDto
        {
            Title = TestName,
            Text = TestText,
            SongGenres = new List<int> { 1, 2, 3, 11 },
            Id = _testSongId
        };

        // act:
        var response = await _updateModel.UpdateNote(song);

        // assert:
        // TODO поправь - где-то в стабе я меняю местами text и title:
        Assert.AreEqual(TestText, response.TitleResponse);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }
}
