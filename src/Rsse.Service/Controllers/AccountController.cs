using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchEngine.Common;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Managers;
using Swashbuckle.AspNetCore.Annotations;
using ZstdSharp.Unsafe;
using static SearchEngine.Common.ControllerMessages;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер авторизации
/// </summary>
[ApiController, Route("account")]
public class AccountController(
    IWebHostEnvironment env,
    ILogger<AccountController> logger)
    : ControllerBase
{
    private const string SameSiteLax = "samesite=lax";
    private const string SameSiteNone = "samesite=none; secure; partitioned";

    /// <summary>
    /// Авторизоваться в системе
    /// </summary>
    /// <param name="email">email</param>
    /// <param name="password">пароль</param>
    /// <param name="returnUrl">пиздец</param>
    /// <returns>объект OkObjectResult с результатом</returns>
    [HttpGet("login")]
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

        var loginDto = new LoginDto { Email = email, Password = password };
        var response = await TryLogin(loginDto);
        return response == "[Ok]" ? LoginOkMessage : Unauthorized(response);
    }

    /// <summary>
    /// Выйти из системы
    /// </summary>
    [HttpGet("logout")]
    public async Task<ActionResult<string>> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        ModifyCookie();

        return LogOutMessage;
    }

    /// <summary/> Проверить, авторизован ли запрос
    [ApiExplorerSettings(IgnoreApi = !Constants.IsDebug)]
    [HttpGet("check"), Authorize]
    public ActionResult CheckAuth()
    {
        return Ok(new
        {
            Username = User.Identity?.Name
        });
    }

    /// <summary/> Обновить логин и пароль
    [HttpGet("update"), Authorize]
    public async Task<ActionResult> UpdateCredos([FromQuery] UpdateCredosRequest credos)
    {
        var scopedProvider = HttpContext.RequestServices;
        var repo = scopedProvider.GetRequiredService<IDataRepository>();
        await repo.UpdateCredos(credos);
        await Logout();
        return Ok("updated");
    }

    /// <summary>
    /// Вход в систему, аутентификация на основе кук
    /// </summary>
    /// <param name="loginDto">данные для авторизации</param>
    private async Task<string> TryLogin(LoginDto loginDto)
    {
        var scopedProvider = HttpContext.RequestServices;
        try
        {
            var identity = await new LoginManager(scopedProvider).TrySignInWith(loginDto);

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
    /// Модифицировать куки при разработке
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
