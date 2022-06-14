using System.Security.Claims;
using RandomSongSearchEngine.Data;
using RandomSongSearchEngine.Data.DTO;
using RandomSongSearchEngine.Data.Repository.Contracts;

namespace RandomSongSearchEngine.Service.Models;

public class LoginModel
{
    private readonly IServiceScope _scope;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(IServiceScope scope)
    {
        _scope = scope;
        
        _logger = _scope.ServiceProvider.GetRequiredService<ILogger<LoginModel>>();
    }

    public async Task<ClaimsIdentity?> TryLogin(LoginDto login)
    {
        try
        {
            if (login.Email == null || login.Password == null)
            {
                return null;
            }

            await using var repo = _scope.ServiceProvider.GetRequiredService<IDataRepository>();
            
            var user = await repo.GetUser(login);
            
            if (user == null)
            {
                return null;
            }

            var claims = new List<Claim> {new(ClaimsIdentity.DefaultNameClaimType, login.Email)};
            
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
            _logger.LogError(ex, "[LoginModel: System Error]");
            
            return null;
        }
    }
}