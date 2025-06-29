using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Data.Dto;
using Rsse.Domain.Service.Api;
using Rsse.Tests.Units.Infra;
using CreateService = Rsse.Domain.Service.Api.CreateService;

namespace Rsse.Tests.Units;

[TestClass]
public class CreateTests
{
    public required CreateService CreateService;
    public required ServiceProviderStub ServiceProviderStub;
    public required IDataRepository Repository;

    private readonly CancellationToken _token = CancellationToken.None;

    [TestInitialize]
    public void Initialize()
    {
        ServiceProviderStub = new ServiceProviderStub();
        Repository = ServiceProviderStub.Scope.ServiceProvider.GetRequiredService<IDataRepository>();
        CreateService = new CreateService(Repository);
    }

    [TestCleanup]
    public void Cleanup()
    {
        ServiceProviderStub.Dispose();
    }

    [TestMethod]
    public async Task CreateManager_ShouldReports_ExpectedTagsCount()
    {
        // arrange & act:
        var resultDto = await Repository.ReadTags(_token);

        // assert:
        Assert.AreEqual(FakeCatalogRepository.TagNameList.Count, resultDto.Count);
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
        var repo = ServiceProviderStub.Provider.GetRequiredService<IDataRepository>();

        var actualDto = await new UpdateService(repo).GetNoteWithTagsForUpdate(responseDto.NoteIdExchange, _token);

        // assert:
        responseDto.ErrorMessage.Should().BeNull();
        responseDto.Title.Should().Be("[OK]");
        Assert.AreEqual(requestDto.Title, actualDto.Title);
    }
}
