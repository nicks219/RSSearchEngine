using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Managers;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.Repo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class CreateTests
{
    public required CreateManager CreateManager;
    public required ServiceProviderStub<CreateManager> Host;

    [TestInitialize]
    public void Initialize()
    {
        Host = new ServiceProviderStub<CreateManager>();
        var repo = Host.Scope.ServiceProvider.GetRequiredService<IDataRepository>();
        var managerLogger = Host.Scope.ServiceProvider.GetRequiredService<ILogger<CreateManager>>();
        CreateManager = new CreateManager(repo, managerLogger);
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
        var requestDto = new NoteRequestDto
        (
            TagsCheckedRequest: [1, 2, 3],
            TitleRequest: "test: title",
            TextRequest: "test: text",
            NoteIdExchange: default
        );

        // act:
        var responseDto = await CreateManager.CreateNote(requestDto);
        var repo = Host.Provider.GetRequiredService<IDataRepository>();
        var managerLogger = Host.Provider.GetRequiredService<ILogger<UpdateManager>>();

        var actualDto = await new UpdateManager(repo, managerLogger).GetNoteWithTagsForUpdate(responseDto.NoteIdExchange);

        // assert:
        responseDto.CommonErrorMessageResponse.Should().BeNull();
        responseDto.TitleResponse.Should().Be("[OK]");
        // todo: меняй таплы на нормальные контейнеры - начни со слоя репозитория
        Assert.AreEqual(requestDto.TitleRequest, actualDto.TitleResponse);
    }
}
