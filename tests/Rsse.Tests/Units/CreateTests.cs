using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Services;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.Repo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class CreateTests
{
    public required CreateService CreateService;
    public required ServiceProviderStub Stub;
    public required IDataRepository Repository;

    private readonly CancellationToken _token = CancellationToken.None;

    [TestInitialize]
    public void Initialize()
    {
        Stub = new ServiceProviderStub();
        Repository = Stub.Scope.ServiceProvider.GetRequiredService<IDataRepository>();
        CreateService = new CreateService(Repository);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Stub.Dispose();
    }

    [TestMethod]
    public async Task CreateManager_ShouldReports_ExpectedTagsCount()
    {
        // arrange & act:
        var resultDto = await Repository.ReadEnrichedTagList(_token);

        // assert:
        Assert.AreEqual(FakeCatalogRepository.TagList.Count, resultDto.Count);
    }

    [TestMethod]
    public async Task CreateService_ShouldCreatValidNote_Correctly()
    {
        // arrange:
        var requestDto = new NoteRequestDto
        (
            CheckedTags: [1, 2, 3],
            Title: "test: title",
            Text: "test: text",
            NoteIdExchange: default
        );

        // act:
        var responseDto = await CreateService.CreateNote(requestDto, _token);
        var repo = Stub.Provider.GetRequiredService<IDataRepository>();

        var actualDto = await new UpdateService(repo).GetNoteWithTagsForUpdate(responseDto.NoteIdExchange, _token);

        // assert:
        responseDto.ErrorMessage.Should().BeNull();
        responseDto.Title.Should().Be("[OK]");
        Assert.AreEqual(requestDto.Title, actualDto.Title);
    }
}
