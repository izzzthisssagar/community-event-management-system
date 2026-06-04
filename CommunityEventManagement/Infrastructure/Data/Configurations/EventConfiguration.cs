using CommunityEventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityEventManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EventConfiguration sets up how the Event entity maps to the database using the Fluent API.
/// I chose the Fluent API with separate IEntityTypeConfiguration classes (instead of data
/// annotations on the entities) because it keeps my entities clean and gives me much more
/// control over things like relationships and indexes. This is the most detailed configuration
/// because the Event sits at the centre of two many-to-many relationships.
/// </summary>
public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        // The primary key is the Id that Event inherited from BaseEntity.
        builder.HasKey(e => e.Id);

        // Basic column rules. IsRequired means NOT NULL, HasMaxLength limits the column size.
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.MaxCapacity).IsRequired();
        builder.Property(e => e.CancellationReason).HasMaxLength(500);

        // I configure ConcurrencyToken as a concurrency token (NOT a row version). I do this on
        // purpose because IsConcurrencyToken() works on both MySQL and SQLite, whereas the row
        // version approach only works on MySQL and would break my SQLite unit tests.
        builder.Property(e => e.ConcurrencyToken).IsConcurrencyToken();

        // These three navigation collections are backed by private fields (_registrations,
        // _venues, _activities). I tell EF Core to read and write the FIELD directly instead of
        // the read-only property, otherwise it would not be able to fill the collections.
        builder.Navigation(e => e.Registrations).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(e => e.Venues).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(e => e.Activities).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Many-to-many: an Event can use many Venues and a Venue can host many Events. There is
        // no extra data on this link, so I let EF create a simple junction table "EventVenues".
        builder.HasMany(e => e.Venues)
               .WithMany(v => v.Events)
               .UsingEntity(j => j.ToTable("EventVenues"));

        // Many-to-many: an Event can include many Activities and an Activity can appear in many
        // Events. Again no extra data, so EF creates the junction table "EventActivities".
        builder.HasMany(e => e.Activities)
               .WithMany(a => a.Events)
               .UsingEntity(j => j.ToTable("EventActivities"));
    }
}
