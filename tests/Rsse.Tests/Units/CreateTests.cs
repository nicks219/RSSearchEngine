using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Services;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.Repo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class CreateTests
{
    public required CreateService CreateService;
    public required ServiceProviderStub Host;

    [TestInitialize]
    public void Initialize()
    {
        Host = new ServiceProviderStub();
        var repo = Host.Scope.ServiceProvider.GetRequiredService<IDataRepository>();
        var managerLogger = Host.Scope.ServiceProvider.GetRequiredService<ILogger<CreateService>>();
        CreateService = new CreateService(repo, managerLogger);
    }

    [TestMethod]
    public async Task CreateManager_ShouldReports_ExpectedTagsCount()
    {
        // arrange & act:
        var resultDto = await CreateService.ReadStructuredTagList();

        // assert:
        Assert.AreEqual(FakeCatalogRepository.TagList.Count, resultDto.StructuredTags?.Count);
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
        var repo = Host.Provider.GetRequiredService<IDataRepository>();
        var managerLogger = Host.Provider.GetRequiredService<ILogger<UpdateService>>();

        var actualDto = await new UpdateService(repo, managerLogger).GetNoteWithTagsForUpdate(responseDto.NoteIdExchange);

        // assert:
        responseDto.ErrorMessage.Should().BeNull();
        responseDto.Title.Should().Be("[OK]");
        // todo: меняй таплы на нормальные контейнеры - начни со слоя репозитория
        Assert.AreEqual(requestDto.Title, actualDto.Title);
    }
}
