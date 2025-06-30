using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Rsse.Api.Controllers;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Data.Dto;
using Rsse.Domain.Service.Api;
using Rsse.Domain.Service.Contracts;
using Rsse.Tests.Integration.FakeDb.Extensions;
using Rsse.Tests.Units.Infra;

namespace Rsse.Tests.Units;

[TestClass]
public class CatalogTests
{
    public required ServiceProviderStub ServiceProviderStub;
    public required CatalogService CatalogService;
    public required DeleteService DeleteService;
    public required FakeCatalogRepository Repo;

    private readonly CancellationToken _token = CancellationToken.None;
    private const int NotesPerPage = 10;
    private int _notesCount;

    [TestInitialize]
    public async Task Initialize()
    {
        ServiceProviderStub = new ServiceProviderStub();
        var repo = ServiceProviderStub.Scope.ServiceProvider.GetRequiredService<IDataRepository>();

        CatalogService = new CatalogService(repo);
        DeleteService = new DeleteService(repo);
        Repo = (FakeCatalogRepository)ServiceProviderStub.Provider.GetRequiredService<IDataRepository>();
        Repo.CreateStubData(50, _token);
        _notesCount = await Repo.ReadNotesCount(_token);
    }

    [TestCleanup]
    public void Cleanup()
    {
        ServiceProviderStub.Dispose();
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
    public async Task DeleteController_ShouldThrow_OnInvalidDeleteRequest()
    {
        // arrange:
        const int invalidPageId = -300;
        const int invalidPageNumber = -200;
        var lifetime = Substitute.For<IHostApplicationLifetime>();
        var tokenizerService = Substitute.For<ITokenizerApiClient>();
        var deleteController = new DeleteController(
            lifetime,
            tokenizerService,
            DeleteService,
            CatalogService);

        // act:
        var exception = await TestHelper.GetExpectedExceptionWithAsync<Exception>(() =>
            deleteController.DeleteNote(invalidPageId, invalidPageNumber));

        // assert:
        exception.EnsureNotNull();
        exception.Message.Should().Be("Page number error");
    }

    [TestMethod]
    public async Task CatalogController_ShouldThrow_OnInvalidDeleteRequest()
    {
        // arrange:
        const int invalidPageId = -300;
        const int invalidPageNumber = -200;
        var tokenizer = Substitute.For<ITokenizerApiClient>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var catalogController = new DeleteController(lifetime, tokenizer, DeleteService, CatalogService);

        // act:
        var exception = await TestHelper.GetExpectedExceptionWithAsync<Exception>(() =>
            catalogController.DeleteNote(invalidPageId, invalidPageNumber));

        // assert:
        exception.EnsureNotNull();
        exception.Message.Should().Be("Page number error");
    }

    [TestMethod]
    public async Task CatalogController_ShouldThrow_OnUndefinedRequest()
    {
        // arrange:
        var catalogController = new CatalogController(CatalogService);

        // act:
        var exception =
            await TestHelper.GetExpectedExceptionWithAsync<NullReferenceException>(() =>
                catalogController.NavigateCatalog(null!, _token));

        // assert:
        exception.EnsureNotNull();
        exception.Message.Should().Be("Object reference not set to an instance of an object.");
    }
}
