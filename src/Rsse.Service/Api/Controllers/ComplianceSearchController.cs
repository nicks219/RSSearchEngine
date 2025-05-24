using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Services;
using static SearchEngine.Api.Messages.ControllerErrorMessages;

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
    public ActionResult<ComplianceResponse> GetComplianceIndices(string text)
    {
        try
        {
            var searchIndexes = complianceService.ComputeComplianceIndices(text);

            var response = searchIndexes.Count == 0
                ? new ComplianceResponse()
                : new ComplianceResponse { Res = searchIndexes };

            return Ok(response);
        }
        catch (Exception ex)
        {
            var error = new ComplianceResponse { Error = ComplianceError };
            logger.LogError(ex, ComplianceError);
            return BadRequest(error);
        }
    }
}
