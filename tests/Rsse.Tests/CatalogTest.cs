using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RandomSongSearchEngine.Controllers;
using RandomSongSearchEngine.Data.DTO;
using RandomSongSearchEngine.Data.Repository.Contracts;
using RandomSongSearchEngine.Service.Models;
using RandomSongSearchEngine.Tests.Infrastructure;

namespace RandomSongSearchEngine.Tests;

[TestClass]
public class CatalogTest
{
    private const int SonsPerPage = 10;
    
    private CatalogModel? _catalogModel;

    private int _songsCount;
    
    [TestInitialize]
    public void Initialize()
    {
        FakeLoggerErrors.ExceptionMessage = "";
        
        FakeLoggerErrors.LogErrorMessage = "";
        
        var host = new TestHost<CatalogModel>();
        
        _catalogModel = new CatalogModel(host.ServiceScope);
        
        var repo = host.ServiceProvider.GetRequiredService<IDataRepository>();
        
        _songsCount = repo.ReadAllSongs().Count();
    }

    [TestMethod]
    public async Task Model_ShouldReadCatalogPage()
    {
        // [TODO]: поднять для тестов хост с тестовой бд
        var response = await _catalogModel!.ReadCatalogPageAsync(1);

        Assert.AreEqual( SonsPerPage, response.CatalogPage?.Count);
        Assert.AreEqual( _songsCount, response.SongsCount);
    }

    [TestMethod]
    public async Task Model_ShouldNavigateForward()
    {
        const int page = 1;
        const int forwardConst = 2;
        
        var request = new CatalogDto {NavigationButtons = new List<int> {forwardConst}, PageNumber = page};
        var response = await _catalogModel!.NavigateCatalogAsync(request);

        response.PageNumber
            .Should()
            .Be(page + 1);
    }

    [TestMethod]
    public async Task ModelInvalidRequest_ShouldLoggingError()
    {
        var frontRequest = new CatalogDto {NavigationButtons = new List<int> {1000, 2000}};
        var result = await _catalogModel!.NavigateCatalogAsync(frontRequest);

        Assert.AreEqual("[CatalogModel: OnPost Error]", result.ErrorMessage);
    }

    [TestMethod]
    public async Task ModelNullRequest_ShouldLoggingError()
    {
        _ = await _catalogModel!.NavigateCatalogAsync(null!);
        
        Assert.AreEqual("[CatalogModel: OnPost Error]", FakeLoggerErrors.LogErrorMessage);
    }

    [TestMethod]
    public async Task ModelInvalidRequest_ShouldResponseZeroSongs()
    {
        var response = await _catalogModel!.DeleteSongAsync(-300, -200);
        
        Assert.AreEqual(0, response.SongsCount);
    }

    [TestMethod]
    public async Task ControllerThrowsException_ShouldLogError()
    {
        var mockLogger = Substitute.For<ILogger<CatalogController>>();
        var fakeServiceScopeFactory = Substitute.For<IServiceScopeFactory>();
        fakeServiceScopeFactory.When(s => s.CreateScope()).Do(i => throw new Exception());
        var catalogController = new CatalogController(fakeServiceScopeFactory, mockLogger);

        _ = await catalogController.NavigateCatalogAsync(null!);

        mockLogger.Received().LogError(Arg.Any<Exception>(), "[CatalogController: OnPost Error]");
    }

    [TestMethod]
    public async Task ControllerDeleteInvalidRequest_ShouldResponseNull()
    {
        var logger = Substitute.For<ILogger<CatalogController>>();
        var factory = new CustomServiceScopeFactory(new TestHost<CatalogModel>().ServiceProvider);
        var catalogController = new CatalogController(factory, logger);

        var response = (await catalogController.OnDeleteSongAsync(-300, -200)).Value;

        Assert.AreEqual(null, response?.CatalogPage);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }
}
