using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Common.Auth;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;

namespace SearchEngine.Models;

/// <summary>
/// Функционал авторизации
/// </summary>
public class LoginModel(IServiceScope scope)
{
    private const string SignInError = $"[{nameof(LoginModel)}: {nameof(SignIn)} system error]";

    private readonly IDataRepository _repo = scope.ServiceProvider.GetRequiredService<IDataRepository>();
    private readonly ILogger<LoginModel> _logger = scope.ServiceProvider.GetRequiredService<ILogger<LoginModel>>();

    /// <summary>
    /// Войти в систему
    /// </summary>
    /// <param name="login">данные для авторизации</param>
    /// <returns>объект содержащий подверждение идентичности</returns>
    public async Task<ClaimsIdentity?> SignIn(LoginDto login)
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
