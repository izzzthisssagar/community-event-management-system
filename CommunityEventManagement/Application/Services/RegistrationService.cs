using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;

namespace CommunityEventManagement.Application.Services;

/// <summary>
/// IRegistrationService is the contract for the registration business logic — signing participants
/// up to events and cancelling those sign-ups.
/// </summary>
public interface IRegistrationService
{
    // Registers a participant for an event and returns the new registration.
    Task<Registration> RegisterAsync(Guid guidEventId, Guid guidParticipantId);

    // Cancels an existing registration.
    Task CancelRegistrationAsync(Guid guidRegistrationId, string sReason);

    // Gets all of one participant's registrations.
    Task<List<Registration>> GetByParticipantAsync(Guid guidParticipantId);

    // Gets all of one event's registrations.
    Task<List<Registration>> GetByEventAsync(Guid guidEventId);
}

/// <summary>
/// RegistrationService holds the logic for registering participants for events. This is the class
/// where most of my custom exceptions get thrown, because registering has several rules that can
/// fail (the event might not exist, it might be cancelled, the participant might already be
/// registered, or the event might be full).
/// </summary>
public class RegistrationService : IRegistrationService
{
    private readonly IEventRepository _erEventRepository;
    private readonly IParticipantRepository _prParticipantRepository;
    private readonly IRegistrationRepository _rrRegistrationRepository;

    public RegistrationService(
        IEventRepository erEventRepository,
        IParticipantRepository prParticipantRepository,
        IRegistrationRepository rrRegistrationRepository)
    {
        _erEventRepository = erEventRepository;
        _prParticipantRepository = prParticipantRepository;
        _rrRegistrationRepository = rrRegistrationRepository;
    }

    public async Task<Registration> RegisterAsync(Guid guidEventId, Guid guidParticipantId)
    {
        // Step 1: load the event. If it does not exist, I cannot register anyone for it.
        Event? targetEvent = await _erEventRepository.GetByIdAsync(guidEventId);
        if (targetEvent is null)
        {
            throw new EventNotFoundException($"No event was found with Id '{guidEventId}'.");
        }

        // Step 2: load the participant. If they do not exist, I cannot register them.
        Participant? participant = await _prParticipantRepository.GetByIdAsync(guidParticipantId);
        if (participant is null)
        {
            throw new EntityNotFoundException($"No participant was found with Id '{guidParticipantId}'.");
        }

        // Step 3: a cancelled event is closed, so no one can register for it.
        if (targetEvent.IsCancelled)
        {
            throw new EventManagementException($"You cannot register for '{targetEvent.Name}' because it has been cancelled.");
        }

        // Step 4: let the Event entity apply its own rules. AddRegistration will throw a
        // DuplicateRegistrationException or a VenueCapacityExceededException if a rule is broken,
        // and otherwise it creates and returns the new registration.
        Registration newRegistration = targetEvent.AddRegistration(participant, "Confirmed");

        // Step 5: save the new registration to the database.
        await _rrRegistrationRepository.AddAsync(newRegistration);

        return newRegistration;
    }

    public async Task CancelRegistrationAsync(Guid guidRegistrationId, string sReason)
    {
        Registration? registration = await _rrRegistrationRepository.GetByIdAsync(guidRegistrationId);
        if (registration is null)
        {
            throw new EntityNotFoundException($"No registration was found with Id '{guidRegistrationId}'.");
        }

        // Cancel through the registration's own Cancel() method (from ICancelable), then save.
        registration.Cancel(sReason);
        await _rrRegistrationRepository.UpdateAsync(registration);
    }

    public async Task<List<Registration>> GetByParticipantAsync(Guid guidParticipantId)
    {
        return await _rrRegistrationRepository.GetByParticipantAsync(guidParticipantId);
    }

    public async Task<List<Registration>> GetByEventAsync(Guid guidEventId)
    {
        return await _rrRegistrationRepository.GetByEventAsync(guidEventId);
    }
}
