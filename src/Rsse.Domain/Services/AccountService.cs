using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Service.Configuration;

namespace SearchEngine.Services;

/// <summary>
/// Функционал авторизации.
/// </summary>
public class AccountService(IDataRepository repo)
{
    /// <summary>
    /// Войти в систему.
    /// </summary>
    /// <param name="credentialsRequest">Данные авторизации.</param>
    /// <returns>Контейнер с подверждением идентичности.</returns>
    public async Task<ClaimsIdentity?> TrySignInWith(CredentialsRequestDto credentialsRequest)
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

    /// <summary/> Обновить логин и пароль.
    public async Task UpdateCredos(UpdateCredosRequestDto credosForUpdate)
    {
        await repo.UpdateCredos(credosForUpdate);
    }
}
