using CommunityEventManagement.Domain.Entities;

namespace CommunityEventManagement.Domain.Interfaces;

/// <summary>
/// IActivityRepository is the contract for all database operations to do with activities.
/// Because Activity is an abstract class with three subclasses stored using Table-Per-Hierarchy,
/// this interface can return the base Activity type and EF Core gives back the correct subclass
/// automatically. I also added type-specific methods (like GetWorkshopsAsync) to show how I can
/// query just one subclass when I need to.
/// </summary>
public interface IActivityRepository
{
    // Gets every activity of every type. Each item comes back as its real subclass.
    Task<List<Activity>> GetAllAsync();

    // Gets a single activity by Id, or null if not found.
    Task<Activity?> GetByIdAsync(Guid guidId);

    // Gets only the workshop activities, using EF Core's OfType filter on the TPH hierarchy.
    Task<List<WorkshopActivity>> GetWorkshopsAsync();

    // Adds a new activity (of any subclass).
    Task AddAsync(Activity newActivity);

    // Updates an existing activity.
    Task UpdateAsync(Activity updatedActivity);

    // Deletes the activity with the given Id.
    Task DeleteAsync(Guid guidId);
}
