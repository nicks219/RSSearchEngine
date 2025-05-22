using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    [TestInitialize]
    public void Initialize()
    {
        Stub = new ServiceProviderStub();
        Repository = Stub.Scope.ServiceProvider.GetRequiredService<IDataRepository>();
        var managerLogger = Stub.Scope.ServiceProvider.GetRequiredService<ILogger<CreateService>>();
        CreateService = new CreateService(Repository, managerLogger);
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
        var resultDto = await Repository.ReadEnrichedTagList();

        // assert:
        Assert.AreEqual(FakeCatalogRepository.TagList.Count, resultDto.Count);
    }

    [TestMethod]
    public async Task CreateManager_ShouldCreatValidNote_Correctly()
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
        var responseDto = await CreateService.CreateNote(requestDto);
        var repo = Stub.Provider.GetRequiredService<IDataRepository>();
        var managerLogger = Stub.Provider.GetRequiredService<ILogger<UpdateService>>();

        var actualDto = await new UpdateService(repo, managerLogger).GetNoteWithTagsForUpdate(responseDto.NoteIdExchange);

        // assert:
        responseDto.ErrorMessage.Should().BeNull();
        responseDto.Title.Should().Be("[OK]");
        // todo: меняй таплы на нормальные контейнеры - начни со слоя репозитория
        Assert.AreEqual(requestDto.Title, actualDto.Title);
    }
}
