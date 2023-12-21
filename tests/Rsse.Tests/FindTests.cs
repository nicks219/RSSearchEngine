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
using SearchEngine.Infrastructure.Tokenizer;
using SearchEngine.Tests.Infrastructure;

namespace SearchEngine.Tests;

[TestClass]
public class FindTests
{
    // private const string Text = "аблака белагривыи лашатки";
    // private const string Text = "Чёрт с ними! За столом сидим, поём, пляшем…";
    private const string Text = "чорт з ным зо сталом";

    [TestMethod]
    // public void ControllerDeleteInvalidRequest_ShouldResponseNull()
    public void FindIncorrectTypedText_OnStubDatabase_ShouldReturn_ExpectedNoteWeights()
    {
        // arrange:
        var logger = Substitute.For<ILogger<FindController>>();
        var factory = new TestServiceScopeFactory(new TestServiceCollection<TokenizerService>().Provider);
        var findController = new FindController(factory, logger);

        // act:
        var response = (OkObjectResult)findController.Find(Text);
        var serialized = JsonConvert.SerializeObject(response);
        var deserialized = JsonConvert.DeserializeObject<Response>(serialized);

        // надо обдумать, какой пример вообще пригоден для этого теста:

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

