using System.Data;
using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CommunityEventManagement.Infrastructure.Repositories;

/// <summary>
/// RegistrationRepository is the concrete implementation of IRegistrationRepository. It deals with
/// the registrations that link participants to events.
/// </summary>
public class RegistrationRepository : IRegistrationRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _dcfContextFactory;

    public RegistrationRepository(IDbContextFactory<ApplicationDbContext> dcfContextFactory)
    {
        _dcfContextFactory = dcfContextFactory;
    }

    public async Task<Registration?> GetByIdAsync(Guid guidId)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        return await context.Registrations
            .Include(r => r.Event)
            .Include(r => r.Participant)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == guidId);
    }

    public async Task<List<Registration>> GetByParticipantAsync(Guid guidParticipantId)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        return await context.Registrations
            .Include(r => r.Event)
            .AsNoTracking()
            .Where(r => r.ParticipantId == guidParticipantId)
            .OrderByDescending(r => r.RegistrationDate)
            .ToListAsync();
    }

    public async Task<List<Registration>> GetByEventAsync(Guid guidEventId)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        return await context.Registrations
            .Include(r => r.Participant)
            .AsNoTracking()
            .Where(r => r.EventId == guidEventId)
            .OrderBy(r => r.RegistrationDate)
            .ToListAsync();
    }

    public async Task AddAsync(Registration newRegistration)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        // Use a serializable transaction so the capacity COUNT and the INSERT are atomic.
        // Without this, two concurrent requests could both read 0 active registrations for a
        // capacity-1 event, both pass the in-memory check in RegistrationService, and both
        // persist — silently exceeding the event's maximum capacity.
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            int iMaxCapacity = await context.Events
                .Where(e => e.Id == newRegistration.EventId)
                .Select(e => e.MaxCapacity)
                .FirstOrDefaultAsync();

            if (iMaxCapacity > 0)
            {
                int iActiveCount = await context.Registrations
                    .CountAsync(r => r.EventId == newRegistration.EventId && !r.IsCancelled);

                if (iActiveCount >= iMaxCapacity)
                {
                    throw new VenueCapacityExceededException(
                        $"This event has reached its maximum capacity of {iMaxCapacity}.");
                }
            }

            context.Registrations.Add(newRegistration);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(Registration updatedRegistration)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        Registration? existingRegistration = await context.Registrations
            .FirstOrDefaultAsync(r => r.Id == updatedRegistration.Id);

        if (existingRegistration is null)
        {
            throw new EntityNotFoundException($"Could not update because registration with Id '{updatedRegistration.Id}' was not found.");
        }

        existingRegistration.Status = updatedRegistration.Status;

        // If the incoming registration has been cancelled but the stored one has not yet, I run
        // the Cancel() domain method so the cancellation reason and status are all set together.
        if (updatedRegistration.IsCancelled && !existingRegistration.IsCancelled)
        {
            existingRegistration.Cancel(updatedRegistration.CancellationReason ?? "Cancelled by user.");
        }

        existingRegistration.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid guidId)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        Registration? registrationToDelete = await context.Registrations.FirstOrDefaultAsync(r => r.Id == guidId);
        if (registrationToDelete is not null)
        {
            context.Registrations.Remove(registrationToDelete);
            await context.SaveChangesAsync();
        }
    }
}
