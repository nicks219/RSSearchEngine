using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using static SearchEngine.Domain.Configuration.ErrorMessages;

namespace SearchEngine.Domain.Services;

/// <summary>
/// Функционал авторизации
/// </summary>
public class AccountService(IDataRepository repo, ILogger<AccountService> logger)
{
    /// <summary>
    /// Войти в систему
    /// </summary>
    /// <param name="credentialsRequest">данные для авторизации</param>
    /// <returns>объект содержащий подверждение идентичности</returns>
    public async Task<ClaimsIdentity?> TrySignInWith(CredentialsRequestDto credentialsRequest)
    {
        try
        {
            if (string.IsNullOrEmpty(credentialsRequest.Email) || string.IsNullOrEmpty(credentialsRequest.Password))
            {
                return null;
            }

            var user = await repo.GetUser(credentialsRequest);

            if (user == null)
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultNameClaimType, credentialsRequest.Email),
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
            logger.LogError(ex, SignInError);
            return null;
        }
    }

    /// <summary/> Обновить логин и пароль
    public async Task UpdateCredos(UpdateCredosRequestDto credosForUpdate)
    {
        await repo.UpdateCredos(credosForUpdate);
    }
}
