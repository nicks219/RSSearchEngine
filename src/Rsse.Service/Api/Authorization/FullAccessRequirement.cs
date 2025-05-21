using Microsoft.AspNetCore.Authorization;

namespace SearchEngine.Api.Authorization;

/// <summary>
/// Требование получения полных прав авторизации (прав редактирования).
/// </summary>
public class FullAccessRequirement : IAuthorizationRequirement;
