using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Managers;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.Repo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class CreateTests
{
    public required CreateManager CreateManager;
    public required ServicesStubStartup<CreateManager> Host;

    [TestInitialize]
    public void Initialize()
    {
        Host = new ServicesStubStartup<CreateManager>();
        CreateManager = new CreateManager(Host.Scope.ServiceProvider);
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
        {
            TitleRequest = "test: title",
            TextRequest = "test: text",
            TagsCheckedRequest = [1, 2, 3]
        };

        // act:
        var responseDto = await CreateManager.CreateNote(requestDto);
        var actualDto = await new UpdateManager(Host.Provider).GetOriginalNote(responseDto.NoteIdExchange);

        // assert:
        responseDto.CommonErrorMessageResponse.Should().BeNull();
        responseDto.TitleResponse.Should().Be("[OK]");
        // todo: меняй таплы на нормальные контейнеры - начни со слоя репозитория
        Assert.AreEqual(requestDto.TitleRequest, actualDto.TextResponse);
    }
}
