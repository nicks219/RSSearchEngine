using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Data.Dto;
using Rsse.Domain.Exceptions;
using Rsse.Domain.Service.Configuration;
using static Rsse.Domain.Service.Configuration.ServiceErrorMessages;

namespace Rsse.Domain.Service.Api;

/// <summary>
/// Функционал авторизации.
/// </summary>
public class AccountService(IDataRepository repo)
{
    /// <summary>
    /// Попытаться войти в систему.
    /// </summary>
    /// <param name="credentialsRequest">Данные для авторизации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Контейнер с подверждением идентичности.</returns>
    /// <exception cref="RsseUserNotFoundException">Пользователь не найден.</exception>
    /// <exception cref="RsseInvalidCredosException">Некорректные данные авторизации.</exception>
    public async Task<ClaimsIdentity> TrySignInWith(CredentialsRequestDto credentialsRequest, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(credentialsRequest.Email) || string.IsNullOrEmpty(credentialsRequest.Password))
        {
            throw new RsseInvalidCredosException(InvalidCredosError);
        }

        var user = await repo.GetUser(credentialsRequest, cancellationToken);

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
    public async Task UpdateCredos(UpdateCredosRequestDto credosForUpdate, CancellationToken cancellationToken)
    {
        await repo.UpdateCredos(credosForUpdate, cancellationToken);
    }
}
