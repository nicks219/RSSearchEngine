using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Api.Controllers;
using SearchEngine.Api.Services;
using SearchEngine.Data.Contracts;
using SearchEngine.Services;
using SearchEngine.Services.ApiModels;
using SearchEngine.Services.Contracts;
using SearchEngine.Tests.Integration.FakeDb.Extensions;
using SearchEngine.Tests.Units.Infra;
using SearchEngine.Tokenizer.SearchProcessor;

namespace SearchEngine.Tests.Units;

[TestClass]
public class ComplianceTests
{
    private readonly List<SearchType> _searchTypes =
        [SearchType.GinOptimized, SearchType.GinSimple, SearchType.Original, SearchType.GinFast];
    private readonly CancellationToken _token = CancellationToken.None;

    [TestMethod]
    [DataRow("чорт з ным зо сталом", """{"res":{"1":2.3529411764705883},"error":null}""")]
    [DataRow("чёрт с ними за столом", """{"res":{"1":294.11764705882354},"error":null}""")]
    [DataRow("удача с ними за столом", """{"res":{"1":23.529411764705884},"error":null}""")]

    // optimized может отличаться от original если оптимизация не учитывает порядок слов
    [DataRow("с ними за столом чёрт", """{"res":{"1":23.529411764705884},"error":null}""")]
    public async Task ComplianceController_ShouldReturnExpectedNoteWeights_WhenFindIncorrectTypedTextOnStubData(
        string text, string expected)
    {
        foreach (var searchType in _searchTypes)
        {
            // arrange:
            using var stub = new ServiceProviderStub(searchType);
            var tokenizer = stub.Provider.GetRequiredService<ITokenizerService>();
            var complianceManager = stub.Provider.GetRequiredService<ComplianceSearchService>();

            var complianceController = new ComplianceSearchController(complianceManager);
            complianceController.AddHttpContext(stub.Provider);

            // токенайзер необходимо инициализировать явно, тк активируется из фоновой службы, которая в данном тесте не запущена
            var repo = stub.Provider.GetRequiredService<IDataRepository>();
            var dbDataProvider = new DbDataProvider(repo);
            await tokenizer.Initialize(dbDataProvider, _token);

            // act:
            var actionResult = complianceController.GetComplianceIndices(text, _token);
            var okObjectResult = ((OkObjectResult)actionResult.Result.EnsureNotNull()).Value as ComplianceResponse;
            var serialized = JsonSerializer.Serialize(okObjectResult);
            var deserialized = JsonSerializer.Deserialize<ComplianceResponse>(serialized);

            // assert:
            serialized.Should().Be(expected);
            deserialized.Should().NotBeNull();
            deserialized.Error.Should().BeNull();
            deserialized.Res.Should().NotBeNull();

            deserialized.Res.Keys.Should().NotBeEmpty();
            deserialized.Res.Values.Should().NotBeEmpty();

            deserialized.Res
                .Keys
                .ElementAt(0)
                .Should()
                .Be(1);
        }
    }

    [TestMethod]
    [DataRow("123 456 иии", """{"res":null,"error":null}""")]
    [DataRow("я ты он она", """{"res":null,"error":null}""")]
    [DataRow("a b c d .,/#", """{"res":null,"error":null}""")]
    [DataRow(" ", """{"res":null,"error":null}""")]
    [DataRow("", """{"res":null,"error":null}""")]
    public async Task ComplianceController_ShouldReturnNullResult_WhenFindGarbageTextOnStubData(
        string text, string expected)
    {
        foreach (var searchType in _searchTypes)
        {
            // arrange:
            using var stub = new ServiceProviderStub(searchType);
            var tokenizer = stub.Provider.GetRequiredService<ITokenizerService>();
            var complianceManager = stub.Provider.GetRequiredService<ComplianceSearchService>();

            var complianceController = new ComplianceSearchController(complianceManager);
            complianceController.AddHttpContext(stub.Provider);

            // токенайзер необходимо инициализировать явно, тк активируется из фоновой службы, которая в данном тесте не запущена
            var repo = stub.Provider.GetRequiredService<IDataRepository>();
            var dbDataProvider = new DbDataProvider(repo);
            await tokenizer.Initialize(dbDataProvider, _token);

            // act:
            var actionResult = complianceController.GetComplianceIndices(text, _token);
            var okObjectResult = ((OkObjectResult)actionResult.Result.EnsureNotNull()).Value as ComplianceResponse;
            var serialized = JsonSerializer.Serialize(okObjectResult);
            var deserialized = JsonSerializer.Deserialize<ComplianceResponse>(serialized);

            // assert:
            serialized.Should().Be(expected);
            deserialized.Should().NotBeNull();
            deserialized.Error.Should().BeNull();
            deserialized.Res.Should().BeNull();
        }
    }
}
