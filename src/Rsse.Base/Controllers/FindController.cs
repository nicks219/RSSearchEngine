using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Service.Models;

namespace SearchEngine.Controllers;

[Route("api/find")]
public class FindController : ControllerBase
{
    private const string FindError = $"[{nameof(FindController)}: {nameof(Find)} error: Search Indices May Failed !]";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FindController> _logger;

    public FindController(IServiceScopeFactory scopeFactory, ILogger<FindController> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult Find(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Ok(new { });
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var model = new FindModel(scope);
            var searchIndexes = model.ComputeSearchIndexes(text);
            const double threshold = 0.1D;

            switch (searchIndexes.Count)
            {
                case 0:
                    return Ok(new { });

                // нулевой вес не стоит учитывать если результатов много
                case > 10:
                    searchIndexes = searchIndexes
                        .Where(kv => kv.Value > threshold)
                        .ToDictionary(x => x.Key, x => x.Value);
                    break;
            }

            searchIndexes = searchIndexes
                .OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);

            var response = new OkObjectResult(new { Res = searchIndexes });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FindError);
            return new BadRequestObjectResult(FindError);
        }
    }
}
