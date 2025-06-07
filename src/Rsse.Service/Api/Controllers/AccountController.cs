using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SearchEngine.Data.Dto;
using SearchEngine.Service.ApiModels;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Mapping;
using SearchEngine.Services;
using static SearchEngine.Api.Configuration.ControllerErrorMessages;
using static SearchEngine.Api.Configuration.ControllerMessages;

namespace SearchEngine.Api.Controllers;

/// <summary>
/// Контроллер авторизации.
/// </summary>
[ApiController]
public class AccountController(
    IWebHostEnvironment env,
    AccountService accountService) : ControllerBase
{
    private const string SameSiteLax = "samesite=lax";
    private const string SameSiteNone = "samesite=none; secure; partitioned";

    /// <summary>
    /// Авторизоваться в системе.
    /// </summary>
    /// <param name="email">Электронная почта.</param>
    /// <param name="password">Пароль.</param>
    /// <param name="returnUrl">Параметр челенджа авторизации, разобраться с необходимостью.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpGet(RouteConstants.AccountLoginGetUrl)]
    public async Task<ActionResult<StringResponse>> Login(
        [FromQuery][Required(AllowEmptyStrings = false)] string email,
        [FromQuery][Required(AllowEmptyStrings = false)] string password,
        string? returnUrl, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        if (returnUrl != null)
        {
            var redirect = new StringResponse(Res: RedirectError);
            return Unauthorized(redirect);
        }

        var credentialsRequestDto = new CredentialsRequestDto { Email = email, Password = password };

        var identity = await accountService.TrySignInWith(credentialsRequestDto, cancellationToken);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        ModifyCookie();

        return Ok(new StringResponse(Res: LoginOkMessage));
    }

    /// <summary>
    /// Выйти из системы.
    /// </summary>
    [HttpGet(RouteConstants.AccountLogoutGetUrl)]
    public async Task<ActionResult<StringResponse>> Logout(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

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
    public ActionResult<StringResponse> CheckAuth(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        var result = new StringResponse(Res: User.Identity?.Name);
        return Ok(result);
    }

    /// <summary>
    /// Обновить логин и пароль.
    /// </summary>
    /// <param name="credentials">Данные для обновления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpGet(RouteConstants.AccountUpdateGetUrl), Authorize]
    public async Task<ActionResult<StringResponse>> UpdateCredos(
        [FromQuery][Required(AllowEmptyStrings = false)] UpdateCredentialsRequest credentials,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return StatusCode(503);

        var credosForUpdate = credentials.MapToDto();
        await accountService.UpdateCredos(credosForUpdate, cancellationToken);
        await Logout(cancellationToken);
        var result = new StringResponse(Res: "updated");
        return Ok(result);
    }

    /// <summary>
    /// Модифицировать куки на разработке.
    /// </summary>
    private void ModifyCookie()
    {
        if (env.IsProduction())
        {
            return;
        }

        var setCookie = HttpContext.Response.Headers.SetCookie;
        var asString = setCookie.ToString();
        var modified = asString.Replace(SameSiteLax, SameSiteNone);
        HttpContext.Response.Headers.SetCookie = modified;
    }
}
