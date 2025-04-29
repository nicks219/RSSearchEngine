using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Data.Dto;
using SearchEngine.Models;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.DatabaseRepo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class CreateTests
{
    public required CreateManager CreateManager;

    [TestInitialize]
    public void Initialize()
    {
        var host = new CustomServiceProvider<CreateManager>();
        CreateManager = new CreateManager(host.Scope);
    }

    [TestMethod]
    public async Task CreateManager_ShouldReports_ExpectedTagsCount()
    {
        // arrange & act:
        var resultDto = await CreateManager.ReadStructuredTagList();

        // assert:
        Assert.AreEqual(FakeCatalogRepository.TagList.Count, resultDto.StructuredTagsListResponse?.Count);
    }

    [TestMethod]
    public async Task CreateManager_ShouldCreatValidNote_Correctly()
    {
        // arrange:
        var requestDto = new NoteDto
        {
            TitleRequest = "test title",
            TextRequest = "test text",
            TagsCheckedRequest = [1, 2, 3, 4, 11]
        };

        // act:
        var responseDto = await CreateManager.CreateNote(requestDto);
        var expectedDto = await new UpdateManager(new CustomServiceProvider<UpdateManager>().Scope)
            .GetOriginalNote(responseDto.CommonNoteId);

        // assert:
        Assert.AreEqual(expectedDto.TitleRequest, responseDto.TitleRequest);
    }
}
