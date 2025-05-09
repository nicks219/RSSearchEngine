using System.Collections.Generic;

namespace SearchEngine.Tests.Units.Dto;

// todo: перенести в контроллер
/// <summary>
/// Ответ ручки compliance.
/// </summary>
public class ComplianceResponseTestDto
{
    public Dictionary<string, double>? Res { get; init; }
}

// todo: перенести в контроллер
/// <summary>
/// Ответ ручки migrations
/// </summary>
public class MigrationResponseTestDto
{
    public string? Res { get; set; }
}
