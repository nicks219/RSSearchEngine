using System.Collections.Generic;

namespace SearchEngine.Tests.Units.Dto;

/// <summary>
/// Ответ ручки compliance
/// </summary>
public class ComplianceResponseModel
{
    public required Dictionary<string, double> Res { get; init; }
}
