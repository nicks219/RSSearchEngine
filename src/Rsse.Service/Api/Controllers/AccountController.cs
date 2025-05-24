using System;
using System.Security.Claims;
using System.Threading;
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
using SearchEngine.Exceptions;
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
    /// <param name="returnUrl">Параметр челенджа авторизации, разобраться с необходимостью.</param>
    /// <param name="ct">Токен отмены.</param>
    [HttpGet(RouteConstants.AccountLoginGetUrl)]
    public async Task<ActionResult<StringResponse>> Login([FromQuery] string? email, string? password,
        string? returnUrl, CancellationToken ct)
    {
        try
        {
            if (returnUrl != null)
            {
                var redirect = new StringResponse(Res: RedirectError);
                return Unauthorized(redirect);
            }

            var credentialsRequestDto = new CredentialsRequestDto { Email = email, Password = password };

            var identity = await accountService.TrySignInWith(credentialsRequestDto, ct);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            ModifyCookie();

            return Ok(new StringResponse(Res: LoginOkMessage));
        }
        catch (RsseBaseException ex) when (ex is RsseUserNotFoundException or RsseInvalidCredosException)
        {
            logger.LogWarning(DataError);
            return Unauthorized(new StringResponse(Res: DataError));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LoginError);
            return Unauthorized(new StringResponse(Res: LoginError));
        }
    }

    /// <summary>
    /// Выйти из системы.
    /// </summary>
    [HttpGet(RouteConstants.AccountLogoutGetUrl)]
    public async Task<ActionResult<StringResponse>> Logout()
    {
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ModifyCookie();
            var result = new StringResponse(Res: LogOutMessage);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogoutError);
            return Unauthorized(new StringResponse(Res: LogoutError));
        }
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
    /// <param name="ct">Токен отмены.</param>
    [HttpGet(RouteConstants.AccountUpdateGetUrl), Authorize]
    public async Task<ActionResult<StringResponse>> UpdateCredos(
        [FromQuery] UpdateCredentialsRequest credentials,
        CancellationToken ct)
    {
        try
        {
            var credosForUpdate = credentials.MapToDto();
            await accountService.UpdateCredos(credosForUpdate, ct);
            await Logout();
            var result = new StringResponse(Res: "updated");
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, UpdateCredosError);
            return Unauthorized(new StringResponse(Res: UpdateCredosError));
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
