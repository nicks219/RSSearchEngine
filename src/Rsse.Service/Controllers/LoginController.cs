using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Dto;
using SearchEngine.Models;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер авторизации
/// </summary>

[ApiController, Route("account")]

public class LoginController : ControllerBase
{
    private const string LoginError = $"[{nameof(LoginController)}] {nameof(Login)} system error";
    private const string DataError = $"[{nameof(LoginController)}] credentials error";
    private const string LogOutMessage = $"[{nameof(LoginController)}] {nameof(Logout)}";
    private const string LoginOkMessage = $"[{nameof(LoginController)}] {nameof(Login)}";
    private const string ModifyCookieMessage = $"[{nameof(LoginController)}] {nameof(ModifyCookie)}";

    private const string SameSiteLax = "samesite=lax";
    private const string SameSiteNone = "samesite=none; secure";

    private readonly ILogger<LoginController> _logger;
    private readonly IServiceScopeFactory _scope;
    private readonly IWebHostEnvironment _env;

    public LoginController(IServiceScopeFactory serviceScopeFactory, IWebHostEnvironment env, ILogger<LoginController> logger)
    {
        _logger = logger;
        _scope = serviceScopeFactory;
        _env = env;
    }

    /// <summary>
    /// Авторизоваться в системе
    /// </summary>
    /// <param name="email">email</param>
    /// <param name="password">пароль</param>
    /// <returns>объект OkObjectResult с результатом</returns>
    [HttpGet("login")]
    public async Task<ActionResult<string>> Login(string? email, string? password)
    {
        if (email == null || password == null)
        {
            return Unauthorized("Authorize please");
        }

        var loginDto = new LoginDto(email, password);
        var response = await Login(loginDto);
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

    /// <summary>
    /// Вход в систему, аутентификация на основе кук
    /// </summary>
    /// <param name="loginDto">данные для авторизации</param>
    private async Task<string> Login(LoginDto loginDto)
    {
        using var scope = _scope.CreateScope();
        try
        {
            var identity = await new LoginModel(scope).SignIn(loginDto);

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
            _logger.LogError(ex, LoginError);
            return LoginError;
        }
    }

    /// <summary>
    /// Модифицировать куки при разработке
    /// </summary>
    private void ModifyCookie()
    {
        if (_env.IsProduction())
        {
            return;
        }

        _logger.LogInformation(ModifyCookieMessage);

        var setCookie = HttpContext.Response.Headers.SetCookie;
        var asString = setCookie.ToString();
        var modified = asString.Replace(SameSiteLax, SameSiteNone);
        HttpContext.Response.Headers.SetCookie = modified;
    }
}
