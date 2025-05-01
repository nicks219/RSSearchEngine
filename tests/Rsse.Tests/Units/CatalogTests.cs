using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SearchEngine.Common;
using SearchEngine.Controllers;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Managers;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.DatabaseRepo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class CatalogTests
{
    public required CatalogManager CatalogManager;
    public required FakeCatalogRepository Repo;

    private const int NotesPerPage = 10;
    private int _notesCount;
    private ServicesStubStartup<CatalogManager>? _host;
    private NoopLogger<CatalogManager>? _logger;

    [TestInitialize]
    public void Initialize()
    {
        _host = new ServicesStubStartup<CatalogManager>();
        CatalogManager = new CatalogManager(_host.Scope.ServiceProvider);
        Repo = (FakeCatalogRepository)_host.Provider.GetRequiredService<IDataRepository>();
        Repo.CreateStubData(50);
        _notesCount = Repo.ReadAllNotes().Count();
        _logger = (NoopLogger<CatalogManager>)_host.Provider.GetRequiredService<ILogger<CatalogManager>>();
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
        var request = new CatalogDto { Direction = [forwardMagicNumber], PageNumber = currentPage };

        // act:
        var responseDto = await CatalogManager.NavigateCatalog(request);

        // assert:
        responseDto.PageNumber
            .Should()
            .Be(currentPage + 1);
    }

    [TestMethod]
    public async Task CatalogManager_ShouldLogError_OnUndefinedRequest()
    {
        // arrange & act:
        _ = await CatalogManager.NavigateCatalog(null!);

        // assert:
        Assert.AreEqual(_logger?.Message, ErrorMessages.NavigateCatalogError);
    }

    [TestMethod]
    public async Task CatalogManager_ShouldLogError_OnInvalidRequest()
    {
        // arrange:
        List<int> invalidData = [1000, 2000];
        var request = new CatalogDto { Direction = invalidData };

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
        var catalogController = new CatalogController(logger);

        // act:
        var responseDto = (await catalogController.DeleteNote(invalidPageId, invalidPageNumber)).Value;

        // assert:
        Assert.AreEqual(null, responseDto?.CatalogPage);
    }

    [TestMethod]
    public async Task CatalogController_ShouldLogError_OnUndefinedRequest()
    {
        // arrange:
        var logger = Substitute.For<ILogger<CatalogController>>();
        var catalogController = new CatalogController(logger);

        // act:
        _ = await catalogController.NavigateCatalog(null!);

        // assert:
        logger.Received().LogError(Arg.Any<Exception>(), ControllerMessages.NavigateCatalogError);
    }
}
