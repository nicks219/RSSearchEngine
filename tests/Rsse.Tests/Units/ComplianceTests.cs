using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngine.Api.Controllers;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Contracts;
using SearchEngine.Services;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Units.Infra;

namespace SearchEngine.Tests.Units;

[TestClass]
public class ComplianceTests
{
    private const string Text = "чорт з ным зо сталом";
    private readonly CancellationToken _token = CancellationToken.None;

    [TestMethod]
    public async Task ComplianceController_ShouldReturnExpectedNoteWeights_WhenFindIncorrectTypedTextOnStubData()
    {
        // arrange:
        using var stub = new ServiceProviderStub();
        var tokenizer = stub.Provider.GetRequiredService<ITokenizerService>();
        var complianceManager = stub.Provider.GetRequiredService<ComplianceSearchService>();

        var complianceController = new ComplianceSearchController(complianceManager);
        complianceController.AddHttpContext(stub.Provider);

        // необходимо инициализировать явно, тк активируется из фоновой службы, которая в данном тесте не запущена
        await tokenizer.Initialize(CancellationToken.None);

        // act:
        var actionResult = complianceController.GetComplianceIndices(Text, _token);
        var okObjectResult = ((OkObjectResult)actionResult.Result.EnsureNotNull()).Value as ComplianceResponse;
        var serialized = JsonSerializer.Serialize(okObjectResult);
        var deserialized = JsonSerializer.Deserialize<ComplianceResponse>(serialized);

        // assert:
        serialized.Should().Be("""{"res":{"1":2.3529411764705883},"error":null}""");
        deserialized.Should().NotBeNull();
        deserialized.Res.Should().NotBeNull();

        deserialized.Res
            .Keys
            .ElementAt(0)
            .Should()
            .Be(1);

        deserialized.Res
            .Values
            .ElementAt(0)
            .Should()
            .Be(2.3529411764705883D);
    }
}
