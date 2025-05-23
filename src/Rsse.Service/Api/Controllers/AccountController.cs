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
using SearchEngine.Data.Dto;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Services;
using static SearchEngine.Api.Messages.ControllerErrorMessages;
using static SearchEngine.Api.Messages.ControllerMessages;

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
    public async Task<ActionResult<StringResponse>> Login([FromQuery] string? email, string? password, string? returnUrl)
    {
        if (returnUrl != null)
        {
            var redirect = new StringResponse(Res: "Authorize please: redirect detected");
            return Unauthorized(redirect);
        }

        if (email == null || password == null)
        {
            var empty = new StringResponse(Res: "Authorize please: empty credentials detected");
            return Unauthorized(empty);
        }

        var loginDto = new CredentialsRequestDto { Email = email, Password = password };
        var responseMessage = await TryLogin(loginDto);
        ActionResult result = responseMessage == OkMessage
            ? Ok(new StringResponse(Res: LoginOkMessage))
            : Unauthorized(new StringResponse(Res: responseMessage));
        return result;
    }

    /// <summary>
    /// Выйти из системы.
    /// </summary>
    [HttpGet(RouteConstants.AccountLogoutGetUrl)]
    public async Task<ActionResult<StringResponse>> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        ModifyCookie();
        var result = new StringResponse(Res: LogOutMessage);
        return Ok(result);
    }

    /// <summary>
    /// Проверить, авторизован ли запрос.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
    [HttpGet(RouteConstants.AccountCheckGetUrl), Authorize]
    public ActionResult<StringResponse> CheckAuth()
    {
        var result = new StringResponse(Res: User.Identity?.Name);
        return Ok(result);
    }

    /// <summary>
    /// Обновить логин и пароль.
    /// </summary>
    /// <param name="credentials">Данные для обновления.</param>
    [HttpGet(RouteConstants.AccountUpdateGetUrl), Authorize]
    public async Task<ActionResult<StringResponse>> UpdateCredos([FromQuery] UpdateCredentialsRequest credentials)
    {
        var credosForUpdate = credentials.MapToDto();
        await accountService.UpdateCredos(credosForUpdate);
        await Logout();
        var result = new StringResponse(Res: "updated");
        return Ok(result);
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

            return OkMessage;
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
