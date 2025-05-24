using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Exceptions;
using SearchEngine.Service.Configuration;
using static SearchEngine.Service.Configuration.ServiceErrorMessages;

namespace SearchEngine.Services;

/// <summary>
/// Функционал авторизации.
/// </summary>
public class AccountService(IDataRepository repo)
{
    /// <summary>
    /// Попытаться войти в систему.
    /// </summary>
    /// <param name="credentialsRequest">Данные для авторизации.</param>
    /// <returns>Контейнер с подверждением идентичности.</returns>
    /// <exception cref="RsseUserNotFoundException">Пользователь не найден.</exception>
    /// <exception cref="RsseInvalidCredosException">Некорректные данные авторизации.</exception>
    public async Task<ClaimsIdentity> TrySignInWith(CredentialsRequestDto credentialsRequest)
    {
        if (string.IsNullOrEmpty(credentialsRequest.Email) || string.IsNullOrEmpty(credentialsRequest.Password))
        {
            throw new RsseInvalidCredosException(InvalidCredosError);
        }

        var user = await repo.GetUser(credentialsRequest);

        if (user == null)
        {
            throw new RsseUserNotFoundException(UserNotFoundError);
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
