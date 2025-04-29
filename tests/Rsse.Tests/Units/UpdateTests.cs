using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Tokenizer;
using SearchEngine.Models;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.DatabaseRepo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class UpdateTests
{
    private const string TestName = "0: key";
    private const string TestText = "test text text";
    private int _testNoteId;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private UpdateModel _updateModel;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [TestInitialize]
    public void Initialize()
    {
        var host = new CustomProviderWithLogger<TokenizerService>();
        var provider = new CustomProviderWithLogger<UpdateModel>();
        var findModel = new CompliantModel(host.Scope);

        var repo = (FakeCatalogRepository)host.Provider.GetRequiredService<IDataRepository>();
        repo.CreateStubData(10);

        _testNoteId = findModel.FindNoteId(TestName);
        _updateModel = new UpdateModel(provider.Scope);
    }

    [TestMethod]
    public async Task ModelTagList_ShouldReports_ExpectedGenreCount()
    {
        // arrange & act:
        var response = await _updateModel.GetOriginalNote(1);

        // assert:
        Assert.AreEqual(FakeCatalogRepository.TagList.Count, response.StructuredTagsListResponse?.Count);
    }

    [TestMethod]
    public async Task ModelUpdateTest_ShouldUpdateNoteToExpected()
    {
        // arrange:
        var request = new NoteDto
        {
            TitleRequest = TestName,
            TextRequest = TestText,
            TagsCheckedRequest = new List<int> { 1, 2, 3, 11 },
            CommonNoteId = _testNoteId
        };

        // act:
        var response = await _updateModel.UpdateNote(request);

        // assert:
        // TODO: в стабе поменяны местами text и title, поправь:
        Assert.AreEqual(TestText, response.TitleResponse);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }
}
