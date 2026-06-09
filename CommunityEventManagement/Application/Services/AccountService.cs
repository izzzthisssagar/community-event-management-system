using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Infrastructure.Data;
using CommunityEventManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CommunityEventManagement.Application.Services;

/// <summary>
/// IAccountService is the contract for letting a visitor create their own account.
/// </summary>
public interface IAccountService
{
    Task RegisterAsync(SignUpViewModel vmSignUp);
}

/// <summary>
/// AccountService handles public sign-up. When someone registers, it creates BOTH a login User and
/// a matching Participant profile that share the same email. Linking them by email is what lets the
/// new user immediately register themselves for events and see their own registrations. The
/// password is stored only as a BCrypt hash.
/// </summary>
public class AccountService : IAccountService
{
    private readonly IUserRepository _urUserRepository;
    private readonly IDbContextFactory<ApplicationDbContext> _dcfContextFactory;

    public AccountService(IUserRepository urUserRepository, IDbContextFactory<ApplicationDbContext> dcfContextFactory)
    {
        _urUserRepository = urUserRepository;
        _dcfContextFactory = dcfContextFactory;
    }

    public async Task RegisterAsync(SignUpViewModel vmSignUp)
    {
        // An email can only have one account.
        User? existingUser = await _urUserRepository.GetByEmailAsync(vmSignUp.Email);
        if (existingUser is not null)
        {
            throw new EventManagementException($"An account with the email '{vmSignUp.Email}' already exists.");
        }

        // Use a single DbContext so both the Participant and User rows are saved in one
        // SaveChangesAsync call. If either insert fails, neither is committed — previously
        // two separate repository calls could leave an orphaned Participant with no User.
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        bool bParticipantExists = await context.Participants.AnyAsync(p => p.Email == vmSignUp.Email);
        if (!bParticipantExists)
        {
            Participant newParticipant = new Participant(
                vmSignUp.FirstName, vmSignUp.LastName, vmSignUp.Email, vmSignUp.PhoneNumber);
            context.Participants.Add(newParticipant);
        }

        string sHashedPassword = BCrypt.Net.BCrypt.HashPassword(vmSignUp.Password);
        User newUser = new User($"{vmSignUp.FirstName} {vmSignUp.LastName}", vmSignUp.Email, sHashedPassword, "User");
        context.Users.Add(newUser);

        await context.SaveChangesAsync();
    }
}
