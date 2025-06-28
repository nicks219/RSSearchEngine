using Microsoft.AspNetCore.Authorization;

namespace Rsse.Api.Authorization;

/// <summary>
/// Требование получения полных прав авторизации (прав редактирования).
/// </summary>
public class FullAccessRequirement : IAuthorizationRequirement;
