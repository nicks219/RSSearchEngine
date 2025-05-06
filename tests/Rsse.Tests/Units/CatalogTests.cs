using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SearchEngine.Api.Controllers;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Managers;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.Repo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class CatalogTests
{
    public required CatalogManager CatalogManager;
    public required FakeCatalogRepository Repo;

    private const int NotesPerPage = 10;
    private int _notesCount;

    [TestInitialize]
    public void Initialize()
    {
        var host = new ServiceProviderStub<CatalogManager>();
        var repo = host.Scope.ServiceProvider.GetRequiredService<IDataRepository>();
        var managerLogger = host.Scope.ServiceProvider.GetRequiredService<ILogger<CatalogManager>>();

        CatalogManager = new CatalogManager(repo, managerLogger);
        Repo = (FakeCatalogRepository)host.Provider.GetRequiredService<IDataRepository>();
        Repo.CreateStubData(50);
        _notesCount = Repo.ReadAllNotes().Count();
    }

    [TestMethod]
    public async Task CatalogManager_ShouldRead_ExistingPage()
    {
        // arrange:
        const int existingPage = 1;
        const int totalPages = 50;
        Repo.CreateStubData(totalPages);

        // act:
        var responseDto = await CatalogManager.ReadPage(existingPage);

        // asserts:
        Assert.AreEqual(NotesPerPage, responseDto.CatalogPage?.Count);
        Assert.AreEqual(_notesCount, responseDto.NotesCount);
    }

    [TestMethod]
    public async Task CatalogManager_ShouldNavigate_ForwardDirection()
    {
        const int currentPage = 1;
        const int forwardMagicNumber = 2;

        // arrange:
        Repo.CreateStubData(50);
        var request = new CatalogRequestDto { Direction = [forwardMagicNumber], PageNumber = currentPage };

        // act:
        var responseDto = await CatalogManager.NavigateCatalog(request);

        // assert:
        responseDto.PageNumber
            .Should()
            .Be(currentPage + 1);
    }

    /*[TestMethod]
    public async Task CatalogManager_ShouldLogError_OnUndefinedRequest()
    {
        // arrange & act:
        _ = await CatalogManager.NavigateCatalog(null!);

        // assert:
        Assert.AreEqual(ErrorMessages.NavigateCatalogError, _logger?.Message);
    }*/

    [TestMethod]
    public async Task CatalogManager_ShouldLogError_OnInvalidRequest()
    {
        // arrange:
        List<int> invalidData = [1000, 2000];
        var request = new CatalogRequestDto { Direction = invalidData };

        // act:
        var responseDto = await CatalogManager.NavigateCatalog(request);

        // assert:
        Assert.AreEqual(responseDto.ErrorMessage, ErrorMessages.NavigateCatalogError);
    }

    [TestMethod]
    public async Task CatalogManager_ShouldReturnZeroSongCount_OnInvalidDeleteRequest()
    {
        // arrange & act:
        const int invalidPageId = -300;
        const int invalidPageNumber = -200;
        var responseDto = await CatalogManager.DeleteNote(invalidPageId, invalidPageNumber);

        // assert:
        Assert.AreEqual(0, responseDto.NotesCount);
    }

    [TestMethod]
    public async Task CatalogController_ShouldReturnNullPage_OnInvalidDeleteRequest()
    {
        // arrange:
        const int invalidPageId = -300;
        const int invalidPageNumber = -200;
        var logger = Substitute.For<ILogger<CatalogController>>();
        var managerLogger = Substitute.For<ILogger<CatalogManager>>();
        var repo = Substitute.For<IDataRepository>();
        var tokenizer = Substitute.For<ITokenizerService>();

        var catalogController = new DeleteController(repo, tokenizer, logger, managerLogger);

        // act:
        var responseDto = (await catalogController.DeleteNote(invalidPageId, invalidPageNumber)).Value;

        // assert:
        Assert.AreEqual(null, responseDto.EnsureNotNull().CatalogPage);
    }

    [TestMethod]
    public async Task CatalogController_ShouldLogError_OnUndefinedRequest()
    {
        // arrange:
        var logger = Substitute.For<ILogger<CatalogController>>();
        var managerLogger = Substitute.For<ILogger<CatalogManager>>();
        var repo = Substitute.For<IDataRepository>();

        var catalogController = new CatalogController(repo, logger, managerLogger);

        // act:
        _ = await catalogController.NavigateCatalog(null!);

        // assert:
        logger.Received().LogError(Arg.Any<Exception>(), ControllerMessages.NavigateCatalogError);
    }
}
