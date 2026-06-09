using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Models.ViewModels;

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
    private readonly IParticipantRepository _prParticipantRepository;

    public AccountService(IUserRepository urUserRepository, IParticipantRepository prParticipantRepository)
    {
        _urUserRepository = urUserRepository;
        _prParticipantRepository = prParticipantRepository;
    }

    public async Task RegisterAsync(SignUpViewModel vmSignUp)
    {
        // An email can only have one account.
        User? existingUser = await _urUserRepository.GetByEmailAsync(vmSignUp.Email);
        if (existingUser is not null)
        {
            throw new EventManagementException($"An account with the email '{vmSignUp.Email}' already exists.");
        }

        // Create the participant profile if one does not already exist for this email.
        Participant? existingParticipant = await _prParticipantRepository.GetByEmailAsync(vmSignUp.Email);
        if (existingParticipant is null)
        {
            Participant newParticipant = new Participant(
                vmSignUp.FirstName, vmSignUp.LastName, vmSignUp.Email, vmSignUp.PhoneNumber);
            await _prParticipantRepository.AddAsync(newParticipant);
        }

        // Create the login account with a hashed password and the normal "User" role.
        string sHashedPassword = BCrypt.Net.BCrypt.HashPassword(vmSignUp.Password);
        User newUser = new User($"{vmSignUp.FirstName} {vmSignUp.LastName}", vmSignUp.Email, sHashedPassword, "User");
        await _urUserRepository.AddAsync(newUser);
    }
}
