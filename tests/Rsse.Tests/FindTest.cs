using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;
using RandomSongSearchEngine.Controllers;
using RandomSongSearchEngine.Infrastructure.Cache;
using RandomSongSearchEngine.Tests.Infrastructure;

namespace RandomSongSearchEngine.Tests;

[TestClass]
public class FindTest
{
    private const string Text = "аблака белагривыи лашатки";
    
    [TestMethod]
    // тест проходит на актуальной бд и его ответ зависит от наличия дампа
    public void ControllerDeleteInvalidRequest_ShouldResponseNull()
    {
        var logger = Substitute.For<ILogger<FindController>>();
        var factory = new CustomServiceScopeFactory(new TestHost<CacheRepository>().ServiceProvider);
        var findController = new FindController(factory, logger);

        var response = findController.Find(Text) as OkObjectResult;

        response.Should().NotBeNull();
        var json = JsonConvert.SerializeObject(response);
        var obj = JsonConvert.DeserializeObject<AnonymousResponse>(json);
        
        // {"Value":{"Res":{"270":0.031746031746031744,"228":0.0273972602739726}},"Formatters":[],"ContentTypes":[],"DeclaredType":null,"StatusCode":200}

        obj.Value!.Res?.Keys
            .ElementAt(0)
            .Should()
            .Be("270");
        
        obj.Value!.Res?.Keys
            .ElementAt(1)
            .Should()
            .Be("228");
    }
}

public class AnonymousResponse
{
    [JsonPropertyName("Value")]
    public Value? Value { get; init; }
}

public class Value
{
    [JsonPropertyName("Res")]
    public Dictionary<string, double>? Res { get; init; }
}
