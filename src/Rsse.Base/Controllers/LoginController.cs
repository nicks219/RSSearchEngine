using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Dto;
using SearchEngine.Service.Models;

namespace SearchEngine.Controllers;

[ApiController]
[Route("account")]
public class LoginController : ControllerBase
{
    private const string LoginError = $"[{nameof(LoginController)}: {nameof(Login)} system error]";
    private const string DataError = $"[{nameof(LoginController)}: Data error]";
    private const string LogOutMessage = $"[{nameof(LoginController)}: Data error]";
    private const string LoginOkMessage = $"[{nameof(LoginController)}: {nameof(Login)} Ok]";

    private readonly ILogger<LoginController> _logger;
    private readonly IServiceScopeFactory _scope;

    public LoginController(IServiceScopeFactory serviceScopeFactory, ILogger<LoginController> logger)
    {
        _logger = logger;
        _scope = serviceScopeFactory;
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
        return LogOutMessage;
    }

    private async Task<string> Login(LoginDto model)
    {
        using var scope = _scope.CreateScope();
        try
        {
            var id = await new LoginModel(scope).TryLogin(model);

            if (id == null)
            {
                return DataError;
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
            return "[Ok]";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LoginError);
            return LoginError;
        }
    }
}
