using System.Collections.Generic;

namespace SearchEngine.Tests.Integrations.Dto;

public class ComplianceResponseModel
{
    // ReSharper disable once CollectionNeverUpdated.Global
    // ReSharper disable once InconsistentNaming
    public required Dictionary<string, double> res { get; init; }
}
