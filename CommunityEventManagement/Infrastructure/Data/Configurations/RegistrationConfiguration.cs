using CommunityEventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityEventManagement.Infrastructure.Data.Configurations;

/// <summary>
/// RegistrationConfiguration maps the Registration entity. Registration is the join between an
/// Event and a Participant, but because it also carries its own data (the date and the status)
/// I treat it as a full entity with its own Guid primary key, rather than a hidden join table.
/// I then add a unique index so the same participant can not have two active registrations for
/// the same event at the database level — a second safety net on top of my entity rule.
/// </summary>
public class RegistrationConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        // Registration has its own Guid primary key inherited from BaseEntity.
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status).IsRequired().HasMaxLength(50);
        builder.Property(r => r.RegistrationDate).IsRequired();
        builder.Property(r => r.CancellationReason).HasMaxLength(500);
        builder.Property(r => r.ConcurrencyToken).IsConcurrencyToken();

        // This unique index stops duplicate registrations at the database level. The combination
        // of EventId and ParticipantId must be unique.
        builder.HasIndex(r => new { r.EventId, r.ParticipantId }).IsUnique();

        // Relationship 1: one Event has many Registrations. If an Event is deleted, its
        // registrations are deleted too (cascade).
        builder.HasOne(r => r.Event)
               .WithMany(e => e.Registrations)
               .HasForeignKey(r => r.EventId)
               .OnDelete(DeleteBehavior.Cascade);

        // Relationship 2: one Participant has many Registrations, also with cascade delete.
        builder.HasOne(r => r.Participant)
               .WithMany(p => p.Registrations)
               .HasForeignKey(r => r.ParticipantId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
