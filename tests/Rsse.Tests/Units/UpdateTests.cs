using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Services;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Units.Infra;

namespace SearchEngine.Tests.Units;

[TestClass]
public class UpdateTests
{
    public required ServiceProviderStub ServiceProviderStub;
    public required UpdateService UpdateService;

    private const string Title = "0: key";
    private const string Text = "test text text";

    private readonly CancellationToken _token = CancellationToken.None;

    private int? _testNoteId;

    [TestInitialize]
    public void Initialize()
    {
        ServiceProviderStub = new ServiceProviderStub();
        var repo = (FakeCatalogRepository)ServiceProviderStub.Provider.GetRequiredService<IDataRepository>();

        repo.CreateStubData(10, _token);
        _testNoteId = repo.ReadNoteId(Title, _token);

        UpdateService = new UpdateService(repo);
    }

    [TestCleanup]
    public void Cleanup()
    {
        ServiceProviderStub.Dispose();
    }

    [TestMethod]
    public async Task UpdateService_ShouldReports_ExpectedTagsCount()
    {
        // arrange & act:
        var responseDto = await UpdateService.GetNoteWithTagsForUpdate(1, _token);

        // assert:
        Assert.AreEqual(FakeCatalogRepository.TagList.Count, responseDto.EnrichedTags?.Count);
    }

    [TestMethod]
    public async Task UpdateService_ShouldUpdateNote_Correctly()
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
        var responseDto = await UpdateService.UpdateNote(requestDto, _token);

        // assert:
        Assert.AreEqual(Text, responseDto.Text);
    }
}
