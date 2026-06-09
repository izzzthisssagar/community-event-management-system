using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CommunityEventManagement.Infrastructure.Repositories;

/// <summary>
/// UserRepository is the concrete implementation of IUserRepository. Like my other repositories it
/// uses the IDbContextFactory so each operation gets its own short-lived DbContext.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _dcfContextFactory;

    public UserRepository(IDbContextFactory<ApplicationDbContext> dcfContextFactory)
    {
        _dcfContextFactory = dcfContextFactory;
    }

    public async Task<User?> GetByEmailAsync(string sEmail)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == sEmail);
    }

    public async Task<bool> AnyWithRoleAsync(string sRole)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        return await context.Users.AnyAsync(u => u.Role == sRole);
    }

    public async Task AddAsync(User newUser)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        context.Users.Add(newUser);
        await context.SaveChangesAsync();
    }
}
