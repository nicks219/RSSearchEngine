using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using static SearchEngine.Domain.Configuration.ErrorMessages;

namespace SearchEngine.Domain.Managers;

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
    /// <param name="credentials">данные для авторизации</param>
    /// <returns>объект содержащий подверждение идентичности</returns>
    public async Task<ClaimsIdentity?> TrySignInWith(CredentialsDto credentials)
    {
        try
        {
            if (string.IsNullOrEmpty(credentials.Email) || string.IsNullOrEmpty(credentials.Password))
            {
                return null;
            }

            var user = await _repo.GetUser(credentials);

            if (user == null)
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultNameClaimType, credentials.Email),
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
