using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;
using SearchEngine.Controllers;
using SearchEngine.Engine.Tokenizer;
using SearchEngine.Tests.Units.Mocks;

namespace SearchEngine.Tests.Units;

[TestClass]
public class FindTests
{
    // private const string Text = "аблака белагривыи лашатки";
    // private const string Text = "я ты он она";
    private const string Text = "чорт з ным зо сталом";

    [TestMethod]
    public void FindIncorrectTypedText_OnStubDatabase_ShouldReturn_ExpectedNoteWeights()
    {
        // arrange:
        var logger = Substitute.For<ILogger<ComplianceController>>();
        var factory = new TestServiceScopeFactory(new TestServiceCollection<TokenizerService>().Provider);
        var findController = new ComplianceController(factory, logger);

        // act:
        var response = (OkObjectResult)findController.GetComplianceIndices(Text);
        var serialized = JsonConvert.SerializeObject(response);
        var deserialized = JsonConvert.DeserializeObject<Response>(serialized);

        // assert:
        deserialized.Should().NotBeNull();
        deserialized?.Value.Should().NotBeNull();
        deserialized?.Value?.Res.Should().NotBeNull();

        // {"Value":{"Res":{"270":0.031746031746031744,"228":0.0273972602739726}},"Formatters":[],"ContentTypes":[],"DeclaredType":null,"StatusCode":200}
        deserialized?.Value?.Res?
            .Keys
            .ElementAt(0)
            .Should()
            .Be("1");

        deserialized?.Value?.Res?
            .Values
            .ElementAt(0)
            .Should()
            .Be(2.3529411764705883D);
    }

    public class Response
    {
        [JsonPropertyName("Value")]
        public Value? Value { get; init; }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class Value
    {
        [JsonPropertyName("Res")]
        public Dictionary<string, double>? Res { get; init; }
    }
}

