using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Data.Dto;
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
        var hostTokenizerTyped = new TestServiceCollection<TokenizerService>();
        var hostModelTyped = new TestServiceCollection<UpdateModel>();
        var findModel = new CompliantModel(hostTokenizerTyped.Scope);

        TestCatalogRepository.CreateStubData(10);
        _testNoteId = findModel.FindNoteId(TestName);
        _updateModel = new UpdateModel(hostModelTyped.Scope);
    }

    [TestMethod]
    public async Task ModelTagList_ShouldReports_ExpectedGenreCount()
    {
        // arrange & act:
        var response = await _updateModel.GetOriginalNote(1);

        // assert:
        Assert.AreEqual(TestCatalogRepository.TagList.Count, response.StructuredTagsListResponse?.Count);
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
