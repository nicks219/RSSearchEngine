using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Common;
using SearchEngine.Common.Auth;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;
using static SearchEngine.Common.ErrorMessages;

namespace SearchEngine.Managers;

/// <summary>
/// Функционал авторизации
/// </summary>
public class LoginManager(IServiceProvider scopedProvider)
{
    private readonly IDataRepository _repo = scopedProvider.GetRequiredService<IDataRepository>();
    private readonly ILogger<LoginManager> _logger = scopedProvider.GetRequiredService<ILogger<LoginManager>>();

    /// <summary>
    /// Войти в систему
    /// </summary>
    /// <param name="login">данные для авторизации</param>
    /// <returns>объект содержащий подверждение идентичности</returns>
    public async Task<ClaimsIdentity?> TrySignInWith(LoginDto login)
    {
        try
        {
            if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
            {
                return null;
            }

            var user = await _repo.GetUser(login);

            if (user == null)
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultNameClaimType, login.Email),
                new(Constants.IdInternalClaimType, user.Id.ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            return identity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, SignInError);
            return null;
        }
    }
}
