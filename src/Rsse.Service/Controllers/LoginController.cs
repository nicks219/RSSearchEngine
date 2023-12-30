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
using SearchEngine.Service.Models;

namespace SearchEngine.Controllers;

[ApiController]
[Route("account")]
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

    [HttpGet("logout")]
    public async Task<ActionResult<string>> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        ModifyCookie();

        return LogOutMessage;
    }

    private async Task<string> Login(LoginDto loginDto)
    {
        using var scope = _scope.CreateScope();
        try
        {
            var id = await new LoginModel(scope).TryLogin(loginDto);

            if (id == null)
            {
                return DataError;
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));

            ModifyCookie();

            return "[Ok]";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LoginError);
            return LoginError;
        }
    }

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
