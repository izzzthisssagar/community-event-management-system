using CommunityEventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityEventManagement.Infrastructure.Data.Configurations;

/// <summary>
/// ActivityConfiguration maps the abstract Activity class and its three subclasses
/// (WorkshopActivity, GameActivity, TalkActivity). I use the Table-Per-Hierarchy (TPH) strategy,
/// which means all three types are stored in ONE single "Activities" table. EF Core adds a
/// special "ActivityType" discriminator column so it knows which subclass each row really is.
/// When I load activities back out, EF reads that column and creates the correct subclass object
/// automatically, which is what makes polymorphism work straight from the database.
/// </summary>
public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        // Primary key inherited from BaseEntity.
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
        builder.Property(a => a.DurationMinutes).IsRequired();
        builder.Property(a => a.ConcurrencyToken).IsConcurrencyToken();

        // This is the TPH set-up. The discriminator column is called "ActivityType" and it stores
        // a short text value for each subclass so EF can tell them apart.
        builder.HasDiscriminator<string>("ActivityType")
               .HasValue<WorkshopActivity>("Workshop")
               .HasValue<GameActivity>("Game")
               .HasValue<TalkActivity>("Talk");

        // The Events collection on Activity is backed by a private field, so use field access.
        builder.Navigation(a => a.Events).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
