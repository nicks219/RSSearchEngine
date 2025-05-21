using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Services;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.Repo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class UpdateTests
{
    public required UpdateService UpdateService;

    private const string Title = "0: key";
    private const string Text = "test text text";
    private int? _testNoteId;

    [TestInitialize]
    public void Initialize()
    {
        var host = new ServiceProviderStub();
        var repo = (FakeCatalogRepository)host.Provider.GetRequiredService<IDataRepository>();
        var managerLogger = host.Provider.GetRequiredService<ILogger<UpdateService>>();

        repo.CreateStubData(10);
        _testNoteId = repo.ReadNoteId(Title);

        UpdateService = new UpdateService(repo, managerLogger);
    }

    [TestMethod]
    public async Task UpdateManager_ShouldReports_ExpectedTagsCount()
    {
        // arrange & act:
        var responseDto = await UpdateService.GetNoteWithTagsForUpdate(1);

        // assert:
        Assert.AreEqual(FakeCatalogRepository.TagList.Count, responseDto.StructuredTags?.Count);
    }

    [TestMethod]
    public async Task UpdateManager_ShouldUpdateNoteToExpected()
    {
        // arrange:
        var requestDto = new NoteRequestDto
        (
            CheckedTags: [1, 2, 3, 11],
            Title: Title,
            Text: Text,
            NoteIdExchange: _testNoteId.EnsureNotNull().Value
        );

        // act:
        var responseDto = await UpdateService.UpdateNote(requestDto);

        // assert:
        Assert.AreEqual(Text, responseDto.Text);
    }
}
