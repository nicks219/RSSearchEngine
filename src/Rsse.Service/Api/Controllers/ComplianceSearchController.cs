using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Services;
using static SearchEngine.Domain.Configuration.ControllerMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер обработки индексов соответствия для функционала поиска.
/// </summary>
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class ComplianceSearchController(
    ComplianceSearchService complianceService,
    ILogger<ComplianceSearchController> logger) : ControllerBase
{
    /// <summary>
    /// Получить индексы соответствия заметок поисковому запросу.
    /// </summary>
    /// <param name="text">Строка с поисковым запросом.</param>
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
            const double threshold = 0.1D;
            Dictionary<int, double> searchIndexes = complianceService.ComputeComplianceIndices(text);

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

            // todo: поведение не гарантировано, лучше использовать список
            searchIndexes = searchIndexes
                .OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);

            var response = new OkObjectResult(new { Res = searchIndexes });

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ComplianceError);
            return new BadRequestObjectResult(ComplianceError);
        }
    }
}
