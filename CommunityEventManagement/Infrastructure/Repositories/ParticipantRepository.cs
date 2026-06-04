using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CommunityEventManagement.Infrastructure.Repositories;

/// <summary>
/// ParticipantRepository is the concrete implementation of IParticipantRepository. Like all of my
/// repositories it uses the IDbContextFactory so each operation gets its own short-lived context,
/// which is the safe pattern for Blazor Server.
/// </summary>
public class ParticipantRepository : IParticipantRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _dcfContextFactory;

    public ParticipantRepository(IDbContextFactory<ApplicationDbContext> dcfContextFactory)
    {
        _dcfContextFactory = dcfContextFactory;
    }

    public async Task<List<Participant>> GetAllAsync()
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        return await context.Participants
            .AsNoTracking()
            .OrderBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<Participant?> GetByIdAsync(Guid guidId)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        // I include the registrations and their events so the participant detail view can show
        // everything the person has signed up for.
        return await context.Participants
            .Include(p => p.Registrations)
                .ThenInclude(r => r.Event)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == guidId);
    }

    public async Task<Participant?> GetByEmailAsync(string sEmail)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        return await context.Participants
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Email == sEmail);
    }

    public async Task AddAsync(Participant newParticipant)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        context.Participants.Add(newParticipant);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Participant updatedParticipant)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();

        // Load the real tracked participant and copy the new values onto it. This is safer than
        // attaching a detached object because there are no surprises with the change tracker.
        Participant? existingParticipant = await context.Participants
            .FirstOrDefaultAsync(p => p.Id == updatedParticipant.Id);

        if (existingParticipant is null)
        {
            throw new EntityNotFoundException($"Could not update because participant with Id '{updatedParticipant.Id}' was not found.");
        }

        existingParticipant.FirstName = updatedParticipant.FirstName;
        existingParticipant.LastName = updatedParticipant.LastName;
        existingParticipant.Email = updatedParticipant.Email;
        existingParticipant.PhoneNumber = updatedParticipant.PhoneNumber;
        existingParticipant.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid guidId)
    {
        using ApplicationDbContext context = await _dcfContextFactory.CreateDbContextAsync();
        Participant? participantToDelete = await context.Participants.FirstOrDefaultAsync(p => p.Id == guidId);
        if (participantToDelete is not null)
        {
            context.Participants.Remove(participantToDelete);
            await context.SaveChangesAsync();
        }
    }
}
