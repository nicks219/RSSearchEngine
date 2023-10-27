using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Data.Dto;
using SearchEngine.Service.Models;
using SearchEngine.Tests.Infrastructure;
using SearchEngine.Tests.Infrastructure.DAL;

namespace SearchEngine.Tests;

[TestClass]
public class CreateTests
{
    private CreateModel? _createModel;

    [TestInitialize]
    public void Initialize()
    {
        var host = new TestServiceCollection<CreateModel>();
        _createModel = new CreateModel(host.Scope);
    }

    [TestMethod]
    public async Task ModelTagListTest_ShouldReports_ExpectedGenreCount()
    {
        // arrange & act:
        var result = await _createModel!.ReadTagList();

        // assert:
        Assert.AreEqual(TestDataRepository.TagList.Count, result.GenreListResponse?.Count);
    }

    [TestMethod]
    public async Task ModelCreateTest_ShouldCreatNote_Correctly()
    {
        // arrange:
        var song = new NoteDto
        {
            Title = "test title",
            Text = "test text",
            SongGenres = new List<int> { 1, 2, 3, 4, 11 }
        };

        // act:
        var result = await _createModel!.CreateNote(song);
        var expected = await new UpdateModel(new TestServiceCollection<UpdateModel>().Scope)
            .GetOriginalNote(result.Id);

        // assert:
        Assert.AreEqual(expected.Title, result.Title);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }
}
