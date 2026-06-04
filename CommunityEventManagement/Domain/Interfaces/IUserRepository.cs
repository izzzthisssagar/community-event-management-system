using CommunityEventManagement.Domain.Entities;

namespace CommunityEventManagement.Domain.Interfaces;

/// <summary>
/// IUserRepository is the contract for database operations to do with login accounts (users).
/// It is kept small because the system only needs to look users up by email (for logging in) and
/// add new ones (used by the database seeder when it creates the default admin).
/// </summary>
public interface IUserRepository
{
    // Finds a user by their email address, or returns null if no account uses that email.
    Task<User?> GetByEmailAsync(string sEmail);

    // Returns true if there is at least one user with the given role (used by the seeder).
    Task<bool> AnyWithRoleAsync(string sRole);

    // Adds a new user account.
    Task AddAsync(User newUser);
}
