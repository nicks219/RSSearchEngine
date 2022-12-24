using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RandomSongSearchEngine.Data.DTO;
using RandomSongSearchEngine.Service.Models;

namespace RandomSongSearchEngine.Controllers;

[ApiController]
[Route("account")]
public class LoginController : ControllerBase
{
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
        return response == "[Ok]" ? $"[{nameof(LoginController)}: {nameof(Login)} Ok]" : Unauthorized(response);
    }

    [HttpGet("logout")]
    public async Task<ActionResult<string>> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return $"[{nameof(LoginController)}: {nameof(Logout)}]";
    }

    private async Task<string> Login(LoginDto model)
    {
        using var scope = _scope.CreateScope();
        try
        {
            var id = await new LoginModel(scope).TryLogin(model);
            
            if (id == null)
            {
                return $"[{nameof(LoginController)}: Data error]";
            }
            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
            return "[Ok]";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(LoginController)}: System error]");
            return $"[{nameof(LoginController)}: System error]";
        }
    }
}