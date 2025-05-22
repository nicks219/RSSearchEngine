using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SearchEngine.Api.Controllers;
using SearchEngine.Service.Contracts;
using SearchEngine.Services;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Units.Dto;
using SearchEngine.Tests.Units.Mocks;

namespace SearchEngine.Tests.Units;

[TestClass]
public class ComplianceTests
{
    private const string Text = "чорт з ным зо сталом";

    [TestMethod]
    public async Task ComplianceController_ShouldReturnExpectedNoteWeights_WhenFindIncorrectTypedTextOnStubData()
    {
        // arrange:
        var logger = Substitute.For<ILogger<ComplianceSearchController>>();
        var sp = new ServiceProviderStub();
        var tokenizer = sp.Provider.GetRequiredService<ITokenizerService>();
        var complianceManager = sp.Provider.GetRequiredService<ComplianceSearchService>();

        var complianceController = new ComplianceSearchController(complianceManager, logger);
        complianceController.AddHttpContext(sp.Provider);

        // необходимо инициализировать явно, тк активируется из фоновой службы, которая в данном тесте не запущена
        await tokenizer.Initialize();

        // act:
        var actionResult = complianceController.GetComplianceIndices(Text);
        var anonymousTypeAsResult = ((OkObjectResult)actionResult).Value;
        var serialized = JsonSerializer.Serialize(anonymousTypeAsResult);
        var deserialized = JsonSerializer.Deserialize<ComplianceResponseTestDto>(serialized);

        // assert:
        deserialized.Should().NotBeNull();
        deserialized.Res.Should().NotBeNull();

        deserialized.Res
            .Keys
            .ElementAt(0)
            .Should()
            .Be("1");

        deserialized.Res
            .Values
            .ElementAt(0)
            .Should()
            .Be(2.3529411764705883D);
    }
}
