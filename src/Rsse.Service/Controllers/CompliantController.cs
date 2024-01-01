using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Models;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер для поддержки функционала поиска
/// </summary>

[Route("api/find")]

public class CompliantController : ControllerBase
{
    private const string FindError = $"[{nameof(CompliantController)}] {nameof(GetComplianceIndices)} error: search indices may corrupted";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CompliantController> _logger;

    public CompliantController(IServiceScopeFactory scopeFactory, ILogger<CompliantController> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Получить индексы соответсвия хранимых заметок поисковому запросу
    /// </summary>
    /// <param name="text">строка с поисковым запросом</param>
    /// <returns>объект OkObjectResult с результатом поиска</returns>
    [HttpGet]
    public ActionResult GetComplianceIndices(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Ok(new { });
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var model = new CompliantModel(scope);
            var searchIndexes = model.ComputeComplianceIndices(text);
            const double threshold = 0.1D;

            switch (searchIndexes.Count)
            {
                case 0:
                    return Ok(new { });

                // низкий вес не стоит учитывать если результатов много:
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
