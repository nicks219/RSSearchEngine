using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SearchEngine.Controllers;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Service.Models;
using SearchEngine.Tests.Infrastructure;
using SearchEngine.Tests.Infrastructure.DAL;

namespace SearchEngine.Tests;

[TestClass]
public class CatalogTest
{
    private const int SonsPerPage = 10;
    private CatalogModel? _catalogModel;
    private int _songsCount;
    private TestServiceProvider<CatalogModel>? _host;
    private TestLogger<CatalogModel>? _logger;

    [TestInitialize]
    public void Initialize()
    {
        _host = new TestServiceProvider<CatalogModel>(useStubDataRepository: true);
        _catalogModel = new CatalogModel(_host.ServiceScope);
        var repo = _host.ServiceProvider.GetRequiredService<IDataRepository>();
        TestDataRepository.CreateStubData(50);
        _songsCount = repo.ReadAllNotes().Count();
        _logger = (TestLogger<CatalogModel>)_host.ServiceProvider.GetRequiredService<ILogger<CatalogModel>>();
    }

    [TestMethod]
    public async Task CatalogModel_ShouldRead_Page()
    {
        // arrange:
        TestDataRepository.CreateStubData(50);

        // act:
        var response = await _catalogModel!.ReadCatalogPage(1);

        // asserts:
        Assert.AreEqual(SonsPerPage, response.CatalogPage?.Count);
        Assert.AreEqual(_songsCount, response.SongsCount);
    }

    [TestMethod]
    public async Task CatalogModel_ShouldNavigate_Forward()
    {
        const int page = 1;
        const int forwardConst = 2;

        // arrange:
        TestDataRepository.CreateStubData(50);
        var request = new CatalogDto { NavigationButtons = new List<int> { forwardConst }, PageNumber = page };

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
        Assert.AreEqual(_logger?.ErrorMessage, CatalogModel.NavigateCatalogError);
    }

    [TestMethod]
    public async Task CatalogModel_OnInvalidRequest_ShouldLogError()
    {
        // arrange:
        var request = new CatalogDto { NavigationButtons = new List<int> { 1000, 2000 } };

        // act:
        var result = await _catalogModel!.NavigateCatalog(request);

        // assert:
        Assert.AreEqual(result.ErrorMessage, CatalogModel.NavigateCatalogError);
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
        logger.Received().LogError(Arg.Any<Exception>(), CatalogController.NavigateCatalogError);
    }

    [TestMethod]
    public async Task CatalogModel_DeleteTest_OnInvalidDeleteRequest_ShouldReturn_ZeroSongCount()
    {
        // arrange & act:
        var response = await _catalogModel!.DeleteNote(-300, -200);

        // assert:
        Assert.AreEqual(0, response.SongsCount);
    }

    [TestMethod]
    public async Task CatalogController_DeleteTest_OnInvalidDeleteRequest_ShouldReturnNullPage()
    {
        // arrange:
        var logger = Substitute.For<ILogger<CatalogController>>();
        var factory = new TestServiceScopeFactory(new TestServiceProvider<CatalogModel>().ServiceProvider);
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
