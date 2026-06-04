namespace CommunityEventManagement.Domain.Entities;

/// <summary>
/// The User class represents a login account for the system. To keep things focused on the
/// event management features, I kept this simple: there is an Admin who manages everything and
/// normal users. The password is never stored as plain text — I only ever store a BCrypt hash.
/// </summary>
public class User : BaseEntity
{
    // Private parameterless constructor required by EF Core.
    private User() { }

    /// <summary>
    /// Public constructor for creating a user account. The password that is passed in here is
    /// already the hashed value, not the plain text password.
    /// </summary>
    public User(string sFullName, string sEmail, string sPasswordHash, string sRole)
    {
        FullName = sFullName;
        Email = sEmail;
        PasswordHash = sPasswordHash;
        Role = sRole;
    }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // This holds the hashed password only. I never keep the real password anywhere.
    public string PasswordHash { get; set; } = string.Empty;

    // Role decides what the user is allowed to do. In my system it is either "Admin" or "User".
    public string Role { get; set; } = "User";
}
