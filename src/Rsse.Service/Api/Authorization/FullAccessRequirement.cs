using Microsoft.AspNetCore.Authorization;

namespace SearchEngine.Api.Authorization;

/// <summary>
/// Требования для получения полных прав редактирования.
/// </summary>
public class FullAccessRequirement : IAuthorizationRequirement;
