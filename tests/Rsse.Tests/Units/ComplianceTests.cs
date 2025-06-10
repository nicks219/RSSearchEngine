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
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Contracts;
using SearchEngine.Services;
using SearchEngine.Tests.Integration.FakeDb.Extensions;
using SearchEngine.Tests.Units.Infra;

namespace SearchEngine.Tests.Units;

[TestClass]
public class ComplianceTests
{
    private readonly CancellationToken _token = CancellationToken.None;

    [TestMethod]
    [DataRow("чорт з ным зо сталом", """{"res":{"1":2.3529411764705883},"error":null}""")]
    [DataRow("чёрт с ними за столом", """{"res":{"1":294.11764705882354},"error":null}""")]
    [DataRow("удача с ними за столом", """{"res":{"1":23.529411764705884},"error":null}""")]
    public async Task ComplianceController_ShouldReturnExpectedNoteWeights_WhenFindIncorrectTypedTextOnStubData(
        string text, string expected)
    {
        // arrange:
        using var stub = new ServiceProviderStub();
        var tokenizer = stub.Provider.GetRequiredService<ITokenizerService>();
        var complianceManager = stub.Provider.GetRequiredService<ComplianceSearchService>();

        var complianceController = new ComplianceSearchController(complianceManager);
        complianceController.AddHttpContext(stub.Provider);

        // необходимо инициализировать явно, тк активируется из фоновой службы, которая в данном тесте не запущена
        var repo = stub.Provider.GetRequiredService<IDataRepository>();
        var dbDataProvider = new DbDataProvider(repo);
        await tokenizer.Initialize(dbDataProvider, CancellationToken.None);

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

        // deserialized.Res.Values.ElementAt(0).Should().Be(2.3529411764705883D);
    }
}
