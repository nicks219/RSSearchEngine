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
using SearchEngine.Models;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.DatabaseRepo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class CatalogTests
{
    private const int NotesPerPage = 10;
    private CatalogModel? _catalogModel;
    private int _notesCount;
    private TestServiceCollection<CatalogModel>? _host;
    private TestLogger<CatalogModel>? _logger;

    [TestInitialize]
    public void Initialize()
    {
        _host = new TestServiceCollection<CatalogModel>();
        _catalogModel = new CatalogModel(_host.Scope);
        var repo = _host.Provider.GetRequiredService<IDataRepository>();
        TestCatalogRepository.CreateStubData(50);
        _notesCount = repo.ReadAllNotes().Count();
        _logger = (TestLogger<CatalogModel>)_host.Provider.GetRequiredService<ILogger<CatalogModel>>();
    }

    [TestMethod]
    public async Task CatalogModel_ShouldRead_Page()
    {
        // arrange:
        TestCatalogRepository.CreateStubData(50);

        // act:
        var response = await _catalogModel!.ReadPage(1);

        // asserts:
        Assert.AreEqual(NotesPerPage, response.CatalogPage?.Count);
        Assert.AreEqual(_notesCount, response.NotesCount);
    }

    [TestMethod]
    public async Task CatalogModel_ShouldNavigate_Forward()
    {
        const int page = 1;
        const int forwardConst = 2;

        // arrange:
        TestCatalogRepository.CreateStubData(50);
        var request = new CatalogDto { Direction = new List<int> { forwardConst }, PageNumber = page };

        // act:
        var response = await _catalogModel!.NavigateCatalog(request);

        // assert:
        response.PageNumber
            .Should()
            .Be(page + 1);
    }

    [TestMethod]
    public async Task CatalogModel_OnNullRequest_ShouldLogError()
    {
        // arrange & act:
        _ = await _catalogModel!.NavigateCatalog(null!);

        // assert:
        Assert.AreEqual(_logger?.ErrorMessage, ModelMessages.NavigateCatalogError);
    }

    [TestMethod]
    public async Task CatalogModel_OnInvalidRequest_ShouldLogError()
    {
        // arrange:
        var request = new CatalogDto { Direction = new List<int> { 1000, 2000 } };

        // act:
        var result = await _catalogModel!.NavigateCatalog(request);

        // assert:
        Assert.AreEqual(result.ErrorMessage, ModelMessages.NavigateCatalogError);
    }

    [TestMethod]
    public async Task CatalogController_OnThrow_ShouldLogError()
    {
        // arrange:
        var logger = Substitute.For<ILogger<CatalogController>>();
        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        serviceScopeFactory.When(s => s.CreateScope()).Do(i => throw new Exception());
        var catalogController = new CatalogController(serviceScopeFactory, logger);

        // act:
        _ = await catalogController.NavigateCatalog(null!);

        // assert:
        logger.Received().LogError(Arg.Any<Exception>(), ControllerMessages.NavigateCatalogError);
    }

    [TestMethod]
    public async Task CatalogModel_DeleteTest_OnInvalidDeleteRequest_ShouldReturn_ZeroSongCount()
    {
        // arrange & act:
        var response = await _catalogModel!.DeleteNote(-300, -200);

        // assert:
        Assert.AreEqual(0, response.NotesCount);
    }

    [TestMethod]
    public async Task CatalogController_DeleteTest_OnInvalidDeleteRequest_ShouldReturnNullPage()
    {
        // arrange:
        var logger = Substitute.For<ILogger<CatalogController>>();
        var factory = new TestServiceScopeFactory(new TestServiceCollection<CatalogModel>().Provider);
        var catalogController = new CatalogController(factory, logger);

        // act:
        var response = (await catalogController.DeleteNote(-300, -200)).Value;

        // assert:
        Assert.AreEqual(null, response?.CatalogPage);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }
}
