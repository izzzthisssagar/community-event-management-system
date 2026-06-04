using CommunityEventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityEventManagement.Infrastructure.Data.Configurations;

/// <summary>
/// VenueConfiguration maps the Venue entity to the database. A venue is a place that can host
/// many events, and the many-to-many link itself is set up over in EventConfiguration.
/// </summary>
public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name).IsRequired().HasMaxLength(200);
        builder.Property(v => v.Address).IsRequired().HasMaxLength(300);
        builder.Property(v => v.Capacity).IsRequired();
        builder.Property(v => v.ConcurrencyToken).IsConcurrencyToken();

        // The Events collection is backed by a private field, so use field access.
        builder.Navigation(v => v.Events).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
