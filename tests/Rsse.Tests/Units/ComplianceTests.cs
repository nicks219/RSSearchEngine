using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SearchEngine.Controllers;
using SearchEngine.Engine.Tokenizer;
using SearchEngine.Tests.Units.Mocks;

namespace SearchEngine.Tests.Units;

[TestClass]
public class ComplianceTests
{
    // private const string Text = "аблака белагривыи лашатки";
    // private const string Text = "я ты он она";
    private const string Text = "чорт з ным зо сталом";

    [TestMethod]
    // todo: переименовать, развяжи по stub-бд c тестами токенайзера !
    public void FindIncorrectTypedText_OnStubDatabase_ShouldReturn_ExpectedNoteWeights()
    {
        // arrange:
        var logger = Substitute.For<ILogger<ComplianceController>>();
        var collection = new CustomProviderWithLogger<TokenizerService>();
        var factory = new CustomScopeFactory(collection.Provider);
        var complianceController = new ComplianceController(factory, logger);

        // act:
        var actionResult = complianceController.GetComplianceIndices(Text);
        var anonymousTypeAsResult = ((OkObjectResult) actionResult).Value;
        var serialized = JsonSerializer.Serialize(anonymousTypeAsResult);
        var deserialized = JsonSerializer.Deserialize<ResponseModel>(serialized);

        // assert:
        deserialized.Should().NotBeNull();
        deserialized?.Res.Should().NotBeNull();

        deserialized?.Res
            .Keys
            .ElementAt(0)
            .Should()
            .Be("1");

        deserialized?.Res
            .Values
            .ElementAt(0)
            .Should()
            .Be(2.3529411764705883D);
    }

    public class ResponseModel
    {
        public Dictionary<string, double> Res { get; set; }
    }
}

