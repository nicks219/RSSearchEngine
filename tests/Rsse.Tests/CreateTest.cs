using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RandomSongSearchEngine.Data.DTO;
using RandomSongSearchEngine.Service.Models;
using RandomSongSearchEngine.Tests.Infrastructure;

namespace RandomSongSearchEngine.Tests;

[TestClass]
public class CreateTest
{
    private const int GenresCount = 44;
    
    private CreateModel? _createModel;

    [TestInitialize]
    public void Initialize()
    {
        FakeLoggerErrors.ExceptionMessage = "";
        
        FakeLoggerErrors.LogErrorMessage = "";
        
        var host = new TestHost<CreateModel>();
        
        _createModel = new CreateModel(host.ServiceScope);
    }

    [TestMethod]
    public async Task Model_ShouldReports44Genres()
    {
        var response = await _createModel!.ReadGeneralTagList();
        
        Assert.AreEqual(GenresCount, response.GenreListResponse?.Count);
    }

    [TestMethod]
    public async Task Model_ShouldCreate()
    {
        var song = new NoteDto()
        {
            Title = "test title",
            Text = "test text",
            SongGenres = new List<int> {1, 2, 3, 4, 11}
        };
        var response = await _createModel!.CreateNote(song);
        
        var expected = await new UpdateModel(new TestHost<UpdateModel>().ServiceScope)
            .GetInitialNote(response.Id);
        
        Assert.AreEqual(expected.Title, response.Title);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }
}