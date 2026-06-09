using System.Security.Claims;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CommunityEventManagement.Application.Services;

/// <summary>
/// IAuthService is the contract for the authentication logic — signing a user in and out.
/// </summary>
public interface IAuthService
{
    // Checks the email and password and, if they are correct, signs the user in. Returns true on
    // success and false if the details were wrong.
    Task<bool> LoginAsync(LoginViewModel vmLogin);

    // Signs the current user out.
    Task LogoutAsync();
}

/// <summary>
/// AuthService handles logging users in and out using ASP.NET Core cookie authentication. It looks
/// the user up by email, checks the typed password against the stored BCrypt hash, and if it
/// matches it creates a set of claims (the user's identity) and writes the sign-in cookie.
/// I use IHttpContextAccessor to reach the current HttpContext because the sign-in has to happen
/// during a real HTTP request, which is why the login form posts to my MVC AuthController.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _urUserRepository;
    private readonly IHttpContextAccessor _hcaHttpContextAccessor;

    public AuthService(IUserRepository urUserRepository, IHttpContextAccessor hcaHttpContextAccessor)
    {
        _urUserRepository = urUserRepository;
        _hcaHttpContextAccessor = hcaHttpContextAccessor;
    }

    public async Task<bool> LoginAsync(LoginViewModel vmLogin)
    {
        // Find the account that matches the email. If there is none, login fails.
        Domain.Entities.User? foundUser = await _urUserRepository.GetByEmailAsync(vmLogin.Email);
        if (foundUser is null)
        {
            return false;
        }

        // Check the typed password against the stored hash. BCrypt re-hashes the typed password
        // and compares safely — the real password is never stored or compared directly.
        bool bPasswordCorrect = BCrypt.Net.BCrypt.Verify(vmLogin.Password, foundUser.PasswordHash);
        if (!bPasswordCorrect)
        {
            return false;
        }

        // Build the list of claims. Claims are pieces of information about the signed-in user that
        // I can read later (for example to show their name or to check if they are an admin).
        List<Claim> listClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, foundUser.Id.ToString()),
            new Claim(ClaimTypes.Name, foundUser.FullName),
            new Claim(ClaimTypes.Email, foundUser.Email),
            new Claim(ClaimTypes.Role, foundUser.Role)
        };

        ClaimsIdentity identity = new ClaimsIdentity(listClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);

        // RememberMe decides whether the cookie should survive after the browser is closed.
        AuthenticationProperties authProperties = new AuthenticationProperties
        {
            IsPersistent = vmLogin.RememberMe
        };

        // Write the authentication cookie onto the current response.
        await _hcaHttpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

        return true;
    }

    public async Task LogoutAsync()
    {
        // Remove the authentication cookie, which signs the user out.
        await _hcaHttpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
