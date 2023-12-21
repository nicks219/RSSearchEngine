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
        Assert.AreEqual(TestCatalogRepository.TagList.Count, result.CommonTagsListResponse?.Count);
    }

    [TestMethod]
    public async Task ModelCreateTest_ShouldCreatNote_Correctly()
    {
        // arrange:
        var request = new NoteDto
        {
            TitleRequest = "test title",
            TextRequest = "test text",
            TagsCheckedRequest = new List<int> { 1, 2, 3, 4, 11 }
        };

        // act:
        var response = await _createModel!.CreateNote(request);
        var expected = await new UpdateModel(new TestServiceCollection<UpdateModel>().Scope)
            .GetOriginalNote(response.NoteId);

        // assert:
        Assert.AreEqual(expected.TitleRequest, response.TitleRequest);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }
}
