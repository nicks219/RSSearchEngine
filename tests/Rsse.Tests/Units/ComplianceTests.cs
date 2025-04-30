using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SearchEngine.Controllers;
using SearchEngine.Engine.Tokenizer;
using SearchEngine.Tests.Integrations.Infra;
using SearchEngine.Tests.Units.Mocks;

namespace SearchEngine.Tests.Units;

[TestClass]
public class ComplianceTests
{
    private const string Text = "чорт з ным зо сталом";

    [TestMethod]
    // todo:развяжи по stub-бд c тестами токенайзера
    public void ComplianceController_ShouldReturnExpectedNoteWeights_WhenFindIncorrectTypedTextOnStubData()
    {
        // arrange:
        var logger = Substitute.For<ILogger<ComplianceController>>();
        var host = new ServiceProviderStub<TokenizerService>();
        var complianceController = new ComplianceController(logger);
        complianceController.AddHttpContext(host.Provider);

        // act:
        var actionResult = complianceController.GetComplianceIndices(Text);
        var anonymousTypeAsResult = ((OkObjectResult) actionResult).Value;
        var serialized = JsonSerializer.Serialize(anonymousTypeAsResult);
        var deserialized = JsonSerializer.Deserialize<ResponseModel>(serialized);

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

    public class ResponseModel
    {
        public required Dictionary<string, double> Res { get; set; }
    }
}

