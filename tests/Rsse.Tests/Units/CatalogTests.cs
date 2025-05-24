using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SearchEngine.Api.Controllers;
using SearchEngine.Api.Messages;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Service.Contracts;
using SearchEngine.Services;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.Repo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class CatalogTests
{
    public required ServiceProviderStub Stub;
    public required CatalogService CatalogService;
    public required DeleteService DeleteService;
    public required FakeCatalogRepository Repo;

    private readonly CancellationToken _token = CancellationToken.None;
    private const int NotesPerPage = 10;
    private int _notesCount;

    [TestInitialize]
    public async Task Initialize()
    {
        Stub = new ServiceProviderStub();
        var repo = Stub.Scope.ServiceProvider.GetRequiredService<IDataRepository>();

        CatalogService = new CatalogService(repo);
        DeleteService = new DeleteService(repo);
        Repo = (FakeCatalogRepository)Stub.Provider.GetRequiredService<IDataRepository>();
        Repo.CreateStubData(50, _token);
        _notesCount = await Repo.ReadNotesCount(_token);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Stub.Dispose();
    }

    [TestMethod]
    public async Task CatalogService_ShouldRead_ExistingPage()
    {
        // arrange:
        const int existingPage = 1;
        const int totalPages = 50;
        Repo.CreateStubData(totalPages, _token);

        // act:
        var responseDto = await CatalogService.ReadPage(existingPage, _token);

        // asserts:
        Assert.AreEqual(NotesPerPage, responseDto.CatalogPage?.Count);
        Assert.AreEqual(_notesCount, responseDto.NotesCount);
    }

    [TestMethod]
    public async Task CatalogService_ShouldNavigate_ForwardDirection()
    {
        const int currentPage = 1;
        const int forwardMagicNumber = 2;

        // arrange:
        Repo.CreateStubData(50, _token);
        var request = new CatalogRequestDto { Direction = [forwardMagicNumber], PageNumber = currentPage };

        // act:
        var responseDto = await CatalogService.NavigateCatalog(request, _token);

        // assert:
        responseDto.PageNumber
            .Should()
            .Be(currentPage + 1);
    }

    [TestMethod]
    public async Task CatalogService_ShouldThrow_OnInvalidRequest()
    {
        // arrange:
        List<int> invalidData = [1000, 2000];
        var request = new CatalogRequestDto { Direction = invalidData };

        // act:
        var exception = await TestHelper.GetExpectedExceptionWithAsync<NotSupportedException>(() =>
            CatalogService.NavigateCatalog(request, _token));

        // assert:
        exception.EnsureNotNull();
        exception.Message.Should().Be("[GetDirection] unknown direction");
    }

    [TestMethod]
    public async Task DeleteController_ShouldReturnError_OnInvalidDeleteRequest()
    {
        // arrange:
        const int invalidPageId = -300;
        const int invalidPageNumber = -200;
        var logger = Substitute.For<ILogger<DeleteController>>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();
        var tokenizerService = Substitute.For<ITokenizerService>();
        var deleteController = new DeleteController(
            lifetime,
            tokenizerService,
            DeleteService,
            CatalogService,
            logger);

        // act:
        var catalogResponse = await deleteController.DeleteNote(invalidPageId, invalidPageNumber);

        // assert:
        var response = catalogResponse.Value;
        response.EnsureNotNull();
        response.ErrorMessage.Should().Be(ControllerErrorMessages.DeleteNoteError);
    }

    [TestMethod]
    public async Task CatalogController_ShouldReturnNullPage_OnInvalidDeleteRequest()
    {
        // arrange:
        const int invalidPageId = -300;
        const int invalidPageNumber = -200;
        var logger = Substitute.For<ILogger<DeleteController>>();
        var tokenizer = Substitute.For<ITokenizerService>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var catalogController = new DeleteController(lifetime, tokenizer, DeleteService, CatalogService, logger);

        // act:
        var responseDto = (await catalogController.DeleteNote(invalidPageId, invalidPageNumber)).Value;

        // assert:
        Assert.IsNull(responseDto.EnsureNotNull().CatalogPage);
    }

    [TestMethod]
    public async Task CatalogController_ShouldLogError_OnUndefinedRequest()
    {
        // arrange:
        var logger = Substitute.For<ILogger<CatalogController>>();

        var catalogController = new CatalogController(CatalogService, logger);

        // act:
        _ = await catalogController.NavigateCatalog(null!, _token);

        // assert:
        logger.Received().LogError(Arg.Any<Exception>(), ControllerErrorMessages.NavigateCatalogError);
    }
}
