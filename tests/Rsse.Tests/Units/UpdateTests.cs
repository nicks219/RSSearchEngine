using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Tokenizer;
using SearchEngine.Managers;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.DatabaseRepo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class UpdateTests
{
    public required UpdateManager UpdateManager;

    private const string Title = "0: key";
    private const string Text = "test text text";
    private int _testNoteId;

    [TestInitialize]
    public void Initialize()
    {
        var host = new ServiceProviderStub<TokenizerService>();
        var secondHost = new ServiceProviderStub<UpdateManager>();
        var findModel = new CompliantManager(host.Provider);

        var repo = (FakeCatalogRepository)host.Provider.GetRequiredService<IDataRepository>();
        repo.CreateStubData(10);
        _testNoteId = findModel.FindNoteId(Title);

        UpdateManager = new UpdateManager(secondHost.Provider);
    }

    [TestMethod]
    public async Task UpdateManager_ShouldReports_ExpectedTagsCount()
    {
        // arrange & act:
        var responseDto = await UpdateManager.GetOriginalNote(1);

        // assert:
        Assert.AreEqual(FakeCatalogRepository.TagList.Count, responseDto.StructuredTagsListResponse?.Count);
    }

    [TestMethod]
    public async Task UpdateManager_ShouldUpdateNoteToExpected()
    {
        // arrange:
        var requestDto = new NoteDto
        {
            TitleRequest = Title,
            TextRequest = Text,
            TagsCheckedRequest = [1, 2, 3, 11],
            CommonNoteId = _testNoteId
        };

        // act:
        var responseDto = await UpdateManager.UpdateNote(requestDto);

        // assert:
        Assert.AreEqual(Text, responseDto.TextResponse);
    }
}
