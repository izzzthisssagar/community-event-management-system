using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CommunityEventManagement.Infrastructure.Repositories;

/// <summary>
/// ActivityRepository is the concrete implementation of IActivityRepository. Because Activity uses
/// Table-Per-Hierarchy, when I read activities back EF Core automatically gives me the correct
/// subclass object (WorkshopActivity, GameActivity or TalkActivity), so polymorphism just works.
/// </summary>
public class ActivityRepository : IActivityRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _dcfContextFactory;

    public ActivityRepository(IDbContextFactory<ApplicationDbContext> dcfContextFactory)
    {
        _dcfContextFactory = dcfContextFactory;
    }

    public async Task<List<Activity>> GetAllAsync()
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        // This returns every activity type in one list. Each item is already the right subclass.
        return await context.Activities
            .AsNoTracking()
            .OrderBy(a => a.Title)
            .ToListAsync();
    }

    public async Task<Activity?> GetByIdAsync(Guid guidId)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        return await context.Activities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == guidId);
    }

    public async Task<List<WorkshopActivity>> GetWorkshopsAsync()
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        // OfType<WorkshopActivity> tells EF Core to bring back only the workshop rows from the TPH
        // table. This is a neat way of querying a single subclass out of the hierarchy.
        return await context.Activities
            .OfType<WorkshopActivity>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Activity newActivity)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        context.Activities.Add(newActivity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Activity updatedActivity)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        Activity? existingActivity = await context.Activities.FirstOrDefaultAsync(a => a.Id == updatedActivity.Id);
        if (existingActivity is null)
        {
            throw new EntityNotFoundException($"Could not update because activity with Id '{updatedActivity.Id}' was not found.");
        }

        // Copy the shared values that every activity has.
        existingActivity.Title = updatedActivity.Title;
        existingActivity.DurationMinutes = updatedActivity.DurationMinutes;
        existingActivity.UpdatedAt = DateTime.UtcNow;

        // Then copy the subclass-specific values. I use pattern matching to find out which type it
        // really is. (An activity keeps its type once created — a workshop stays a workshop.)
        if (existingActivity is WorkshopActivity existingWorkshop && updatedActivity is WorkshopActivity updatedWorkshop)
        {
            existingWorkshop.InstructorName = updatedWorkshop.InstructorName;
            existingWorkshop.MaterialsRequired = updatedWorkshop.MaterialsRequired;
        }
        else if (existingActivity is GameActivity existingGame && updatedActivity is GameActivity updatedGame)
        {
            existingGame.MinimumAge = updatedGame.MinimumAge;
            existingGame.EquipmentProvided = updatedGame.EquipmentProvided;
        }
        else if (existingActivity is TalkActivity existingTalk && updatedActivity is TalkActivity updatedTalk)
        {
            existingTalk.SpeakerName = updatedTalk.SpeakerName;
            existingTalk.Topic = updatedTalk.Topic;
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid guidId)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        Activity? activityToDelete = await context.Activities.FirstOrDefaultAsync(a => a.Id == guidId);
        if (activityToDelete is not null)
        {
            context.Activities.Remove(activityToDelete);
            await context.SaveChangesAsync();
        }
    }
}
