using CommunityEventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityEventManagement.Infrastructure.Data.Configurations;

/// <summary>
/// ParticipantConfiguration maps the Participant entity to the database. A participant is a
/// person who can register for events, so it has a one-to-many link to Registration.
/// </summary>
public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Email).IsRequired().HasMaxLength(200);
        builder.Property(p => p.PhoneNumber).HasMaxLength(30);
        builder.Property(p => p.ConcurrencyToken).IsConcurrencyToken();

        // I add a unique index on Email so the same email address can not be used twice.
        builder.HasIndex(p => p.Email).IsUnique();

        // FullName is a calculated C# property only, it is not a real column, so I ignore it.
        builder.Ignore(p => p.FullName);

        // The Registrations collection is backed by a private field, so use field access.
        builder.Navigation(p => p.Registrations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
