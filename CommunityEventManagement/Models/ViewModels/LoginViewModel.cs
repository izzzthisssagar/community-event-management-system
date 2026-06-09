namespace CommunityEventManagement.Models.ViewModels;

/// <summary>
/// LoginViewModel carries the details typed into the login form. The AuthController uses it to
/// check the email and password and sign the user in.
/// </summary>
public class LoginViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}
