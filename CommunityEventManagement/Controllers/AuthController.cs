using CommunityEventManagement.Application.Services;
using CommunityEventManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityEventManagement.Controllers;

/// <summary>
/// AuthController is an MVC controller that handles the actual login and logout. I use a real MVC
/// controller for this (instead of a Blazor component) because cookie authentication has to be
/// written during a normal HTTP request/response, and a Blazor interactive component runs over a
/// SignalR connection where that is not possible. So my login page is a plain HTML form that
/// posts here.
/// The routes are: POST /auth/login, GET /auth/logout, and GET /auth/test.
/// </summary>
[Route("auth")]
public class AuthController : Controller
{
    private readonly IAuthService _asAuthService;

    public AuthController(IAuthService asAuthService)
    {
        _asAuthService = asAuthService;
    }

    /// <summary>
    /// Handles the login form post. It asks the AuthService to check the details and sign the user
    /// in, then redirects to the home page on success or back to the login page on failure.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm] LoginViewModel vmLogin)
    {
        bool bSignedIn = await _asAuthService.LoginAsync(vmLogin);

        if (!bSignedIn)
        {
            // Send the user back to the login page with a flag so it can show an error message.
            return Redirect("/login?error=true");
        }

        // Login worked, so send them to the home page.
        return Redirect("/");
    }

    /// <summary>
    /// Signs the user out and sends them back to the login page.
    /// </summary>
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await _asAuthService.LogoutAsync();
        return Redirect("/login");
    }

    /// <summary>
    /// A simple test route I can visit in the browser (/auth/test) to confirm the controller is
    /// wired up and the routing is working.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Content("AuthController is working correctly.");
    }
}
