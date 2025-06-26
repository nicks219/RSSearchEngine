using System.ComponentModel.DataAnnotations;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using SearchEngine.Services;
using SearchEngine.Services.ApiModels;
using SearchEngine.Services.Configuration;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер обработки индексов соответствия для функционала поиска.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
public class ComplianceSearchController(ComplianceSearchService complianceService) : ControllerBase
{
    /// <summary>
    /// Получить индексы соответствия заметок поисковому запросу.
    /// </summary>
    /// <param name="text">Строка с поисковым запросом.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpGet(RouteConstants.ComplianceIndicesGetUrl)]
    public ActionResult<ComplianceResponse> GetComplianceIndices(
        [FromQuery][Required(AllowEmptyStrings = false)] string text,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        var searchIndexes = complianceService.ComputeComplianceIndices(text, cancellationToken);

        var response = searchIndexes.Count == 0
            ? new ComplianceResponse()
            : new ComplianceResponse { Res = searchIndexes };

        return Ok(response);
    }
}
