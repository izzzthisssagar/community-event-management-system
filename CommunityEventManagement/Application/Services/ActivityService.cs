using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Models.ViewModels;

namespace CommunityEventManagement.Application.Services;

/// <summary>
/// IActivityService is the contract for the activity business logic.
/// </summary>
public interface IActivityService
{
    Task<List<Activity>> GetAllAsync();
    Task<Activity> GetByIdAsync(Guid guidId);
    Task CreateAsync(ActivityViewModel vmActivity);
    Task UpdateAsync(ActivityViewModel vmActivity);
    Task DeleteAsync(Guid guidId);
}

/// <summary>
/// ActivityService holds the logic for managing activities. The most interesting part is the
/// BuildActivityFromViewModel helper, which acts like a small factory: it looks at the chosen
/// ActivityType and creates the correct subclass (Workshop, Game or Talk). This is where my
/// inheritance design pays off.
/// </summary>
public class ActivityService : IActivityService
{
    private readonly IActivityRepository _arActivityRepository;

    public ActivityService(IActivityRepository arActivityRepository)
    {
        _arActivityRepository = arActivityRepository;
    }

    public async Task<List<Activity>> GetAllAsync()
    {
        return await _arActivityRepository.GetAllAsync();
    }

    public async Task<Activity> GetByIdAsync(Guid guidId)
    {
        Activity? foundActivity = await _arActivityRepository.GetByIdAsync(guidId);
        if (foundActivity is null)
        {
            throw new EntityNotFoundException($"No activity was found with Id '{guidId}'.");
        }
        return foundActivity;
    }

    public async Task CreateAsync(ActivityViewModel vmActivity)
    {
        // Use the helper to build the right subclass, then save it.
        Activity newActivity = BuildActivityFromViewModel(vmActivity);
        await _arActivityRepository.AddAsync(newActivity);
    }

    public async Task UpdateAsync(ActivityViewModel vmActivity)
    {
        if (vmActivity.Id is null)
        {
            throw new EventManagementException("Cannot update an activity without an Id.");
        }

        // Load the existing activity so I keep its correct Id and its real subclass type. I then
        // copy the shared values onto it, and use pattern matching to copy the subclass-specific
        // values too. (An activity keeps its type once created — a workshop stays a workshop.)
        Activity existingActivity = await GetByIdAsync(vmActivity.Id.Value);
        existingActivity.Title = vmActivity.Title;
        existingActivity.DurationMinutes = vmActivity.DurationMinutes;

        if (existingActivity is WorkshopActivity existingWorkshop)
        {
            existingWorkshop.InstructorName = vmActivity.InstructorName;
            existingWorkshop.MaterialsRequired = vmActivity.MaterialsRequired;
        }
        else if (existingActivity is GameActivity existingGame)
        {
            existingGame.MinimumAge = vmActivity.MinimumAge;
            existingGame.EquipmentProvided = vmActivity.EquipmentProvided;
        }
        else if (existingActivity is TalkActivity existingTalk)
        {
            existingTalk.SpeakerName = vmActivity.SpeakerName;
            existingTalk.Topic = vmActivity.Topic;
        }

        await _arActivityRepository.UpdateAsync(existingActivity);
    }

    public async Task DeleteAsync(Guid guidId)
    {
        await _arActivityRepository.DeleteAsync(guidId);
    }

    /// <summary>
    /// A small factory helper. Based on the ActivityType chosen on the form, it creates and
    /// returns the matching Activity subclass. This keeps the "which type do I build" decision in
    /// one single place.
    /// </summary>
    private Activity BuildActivityFromViewModel(ActivityViewModel vmActivity)
    {
        // A switch expression keeps this neat. Each branch news up a different subclass.
        Activity builtActivity = vmActivity.ActivityType switch
        {
            "Workshop" => new WorkshopActivity(
                vmActivity.Title, vmActivity.DurationMinutes, vmActivity.InstructorName, vmActivity.MaterialsRequired),

            "Game" => new GameActivity(
                vmActivity.Title, vmActivity.DurationMinutes, vmActivity.MinimumAge, vmActivity.EquipmentProvided),

            "Talk" => new TalkActivity(
                vmActivity.Title, vmActivity.DurationMinutes, vmActivity.SpeakerName, vmActivity.Topic),

            // If somehow an unknown type comes through, I fail loudly with a clear message.
            _ => throw new EventManagementException($"'{vmActivity.ActivityType}' is not a valid activity type.")
        };

        return builtActivity;
    }
}
