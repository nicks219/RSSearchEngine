using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SearchEngine.Common.Auth;

/// <summary>
/// Обработчик правила авторизации <see cref="FullAccessRequirement"/>>
/// </summary>
public class FullAccessRequirementsHandler : AuthorizationHandler<FullAccessRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FullAccessRequirement requirement)
    {
        var claims = context.User.Claims.ToList();
        if (ContainsAdminIdentifier(claims))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }

    /// <summary>Проверить, присутствует ли в списке утверждение с подходящим идентификатором.</summary>
    private static bool ContainsAdminIdentifier(List<Claim> claims)
    {
        var claim = claims.FirstOrDefault(a => a.Type == Constants.IdInternalClaimType);
        var result = claim?.Value == Constants.AdminId;
        return result;
    }
}
