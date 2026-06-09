namespace CommunityEventManagement.Models.ViewModels;

/// <summary>
/// SignUpViewModel carries the details a new visitor types into the sign-up form to create their
/// own account. Creating an account also creates a matching participant profile (they share an
/// email), so the new user can immediately register themselves for events.
/// </summary>
public class SignUpViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
