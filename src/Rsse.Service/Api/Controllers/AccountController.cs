using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchEngine.Api.Mapping;
using SearchEngine.Domain.ApiModels;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Dto;
using SearchEngine.Domain.Services;
using static SearchEngine.Domain.Configuration.ControllerMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер авторизации.
/// </summary>
[ApiController]
public class AccountController(
    IWebHostEnvironment env,
    AccountService accountService,
    ILogger<AccountController> logger) : ControllerBase
{
    private const string SameSiteLax = "samesite=lax";
    private const string SameSiteNone = "samesite=none; secure; partitioned";

    /// <summary>
    /// Авторизоваться в системе.
    /// </summary>
    /// <param name="email">Электронная почта.</param>
    /// <param name="password">Пароль.</param>
    /// <param name="returnUrl">Артефакты легаси-фронта.</param>
    [HttpGet(RouteConstants.AccountLoginGetUrl)]
    public async Task<ActionResult<string>> Login([FromQuery] string? email, string? password, string? returnUrl)
    {
        if (returnUrl != null)
        {
            return Unauthorized("Authorize please: redirect detected");
        }

        if (email == null || password == null)
        {
            return Unauthorized("Authorize please: empty credentials detected");
        }

        var loginDto = new CredentialsRequestDto { Email = email, Password = password };
        var response = await TryLogin(loginDto);
        return response == "[Ok]" ? LoginOkMessage : Unauthorized(response);
    }

    /// <summary>
    /// Выйти из системы.
    /// </summary>
    [HttpGet(RouteConstants.AccountLogoutGetUrl)]
    public async Task<ActionResult<string>> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        ModifyCookie();

        return LogOutMessage;
    }

    /// <summary>
    /// Проверить, авторизован ли запрос.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
    [HttpGet(RouteConstants.AccountCheckGetUrl), Authorize]
    public ActionResult CheckAuth()
    {
        return Ok(new
        {
            Username = User.Identity?.Name
        });
    }

    /// <summary>
    /// Обновить логин и пароль.
    /// </summary>
    /// <param name="credentials">Данные для обновления.</param>
    [HttpGet(RouteConstants.AccountUpdateGetUrl), Authorize]
    public async Task<ActionResult> UpdateCredos([FromQuery] UpdateCredentialsRequest credentials)
    {
        var credosForUpdate = credentials.MapToDto();
        await accountService.UpdateCredos(credosForUpdate);
        await Logout();
        return Ok("updated");
    }

    /// <summary>
    /// Вход в систему, аутентификация на основе кук.
    /// </summary>
    /// <param name="credentialsRequestDto">Данные для авторизации.</param>
    private async Task<string> TryLogin(CredentialsRequestDto credentialsRequestDto)
    {
        try
        {
            var identity = await accountService.TrySignInWith(credentialsRequestDto);

            if (identity == null)
            {
                return DataError;
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            ModifyCookie();

            return "[Ok]";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LoginError);
            return LoginError;
        }
    }

    /// <summary>
    /// Модифицировать куки на разработке.
    /// </summary>
    internal void ModifyCookie()
    {
        if (env.IsProduction())
        {
            return;
        }

        logger.LogInformation(ModifyCookieMessage);

        var setCookie = HttpContext.Response.Headers.SetCookie;
        var asString = setCookie.ToString();
        var modified = asString.Replace(SameSiteLax, SameSiteNone);
        HttpContext.Response.Headers.SetCookie = modified;
    }
}
