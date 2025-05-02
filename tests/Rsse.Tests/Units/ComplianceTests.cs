using System.Linq;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SearchEngine.Api.Controllers;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Tokenizer;
using SearchEngine.Tests.Integrations.Extensions;
using SearchEngine.Tests.Units.Dto;
using SearchEngine.Tests.Units.Mocks;

namespace SearchEngine.Tests.Units;

[TestClass]
public class ComplianceTests
{
    private const string Text = "чорт з ным зо сталом";

    [TestMethod]
    public void ComplianceController_ShouldReturnExpectedNoteWeights_WhenFindIncorrectTypedTextOnStubData()
    {
        // arrange:
        var logger = Substitute.For<ILogger<ComplianceController>>();
        var host = new ServicesStubStartup<TokenizerService>();
        var complianceController = new ComplianceController(logger);
        complianceController.AddHttpContext(host.Provider);

        // необходимо инициализировать явно, тк активируется из фоновой службы, которая в данном тесте не запущена
        var tokenizer = host.Provider.GetRequiredService<ITokenizerService>();
        tokenizer.Initialize();

        // act:
        var actionResult = complianceController.GetComplianceIndices(Text);
        var anonymousTypeAsResult = ((OkObjectResult)actionResult).Value;
        var serialized = JsonSerializer.Serialize(anonymousTypeAsResult);
        var deserialized = JsonSerializer.Deserialize<ComplianceResponseModel>(serialized);

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
