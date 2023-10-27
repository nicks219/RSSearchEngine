using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;

namespace SearchEngine.Service.Models;

public class LoginModel
{
    private const string TryLoginError = $"[{nameof(LoginModel)}: {nameof(TryLogin)} system error]";

    private readonly IDataRepository _repo;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(IServiceScope scope)
    {
        _repo = scope.ServiceProvider.GetRequiredService<IDataRepository>();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<LoginModel>>();
    }

    public async Task<ClaimsIdentity?> TryLogin(LoginDto login)
    {
        try
        {
            if (login.Email == null || login.Password == null)
            {
                return null;
            }

            var user = await _repo.GetUser(login);

            if (user == null)
            {
                return null;
            }

            var claims = new List<Claim> { new(ClaimsIdentity.DefaultNameClaimType, login.Email) };

            var id = new ClaimsIdentity(
                claims,
                "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            // отработает только в классе, унаследованном от ControllerBase
            // await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, TryLoginError);
            return null;
        }
    }
}
