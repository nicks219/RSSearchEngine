using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Rsse.Domain.Service.Configuration;

namespace Rsse.Api.Authorization;

/// <summary>
/// Обработчик правила авторизации <see cref="FullAccessRequirement"/>.
/// </summary>
public class FullAccessRequirementsHandler : AuthorizationHandler<FullAccessRequirement>
{
    /// <summary>Проверить, применимо ли правило авторизации.</summary>
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
        // запись с id = 1 обладает максимальными правами
        var result = claim?.Value == Constants.AdminId;
        return result;
    }
}
