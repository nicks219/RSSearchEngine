using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Managers;
using SearchEngine.Domain.Tokenizer;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.Repo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class UpdateTests
{
    public required UpdateManager UpdateManager;

    private const string Title = "0: key";
    private const string Text = "test text text";
    private int? _testNoteId;

    [TestInitialize]
    public void Initialize()
    {
        var host = new ServiceProviderStub<TokenizerService>();
        var secondHost = new ServiceProviderStub<UpdateManager>();
        var repo = (FakeCatalogRepository)host.Provider.GetRequiredService<IDataRepository>();
        var managerLogger = secondHost.Provider.GetRequiredService<ILogger<UpdateManager>>();

        repo.CreateStubData(10);
        _testNoteId = repo.ReadNoteId(Title);

        UpdateManager = new UpdateManager(repo, managerLogger);
    }

    [TestMethod]
    public async Task UpdateManager_ShouldReports_ExpectedTagsCount()
    {
        // arrange & act:
        var responseDto = await UpdateManager.GetNoteWithTagsForUpdate(1);

        // assert:
        Assert.AreEqual(FakeCatalogRepository.TagList.Count, responseDto.StructuredTagsListResponse?.Count);
    }

    [TestMethod]
    public async Task UpdateManager_ShouldUpdateNoteToExpected()
    {
        // arrange:
        var requestDto = new NoteRequestDto
        (
            TagsCheckedRequest: [1, 2, 3, 11],
            TitleRequest: Title,
            TextRequest: Text,
            NoteIdExchange: _testNoteId.EnsureNotNull().Value
        );

        // act:
        var responseDto = await UpdateManager.UpdateNote(requestDto);

        // assert:
        Assert.AreEqual(Text, responseDto.TextResponse);
    }
}
