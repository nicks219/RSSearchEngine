using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Managers;
using static SearchEngine.Domain.Configuration.ControllerMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер обработки индексов соответствия для функционала поиска
/// </summary>
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class ComplianceSearchController(
    IDataRepository repo,
    ITokenizerService tokenizer,
    ILoggerFactory loggerFactory) : ControllerBase
{
    private readonly ILogger<ComplianceSearchController> _logger = loggerFactory.CreateLogger<ComplianceSearchController>();

    /// <summary>
    /// Получить индексы соответсвия хранимых заметок поисковому запросу
    /// </summary>
    /// <param name="text">строка с поисковым запросом</param>
    /// <returns>объект OkObjectResult с результатом поиска</returns>
    [HttpGet(RouteConstants.ComplianceIndicesGetUrl)]
    public ActionResult GetComplianceIndices(string text)
    {
        var okEmptyResponse = Ok(new { });

        if (string.IsNullOrEmpty(text))
        {
            return okEmptyResponse;
        }

        try
        {
            var manager = new ComplianceSearchManager(repo, tokenizer);
            Dictionary<int, double> searchIndexes = manager.ComputeComplianceIndices(text);
            const double threshold = 0.1D;

            switch (searchIndexes.Count)
            {
                case 0:
                    return okEmptyResponse;

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
            _logger.LogError(ex, ComplianceError);
            return new BadRequestObjectResult(ComplianceError);
        }
    }
}
