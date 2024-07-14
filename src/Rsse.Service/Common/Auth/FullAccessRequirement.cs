using Microsoft.AspNetCore.Authorization;

namespace SearchEngine.Common.Auth;

/// <summary>
/// Требования для получения полных прав редактирования.
/// </summary>
public class FullAccessRequirement : IAuthorizationRequirement;
