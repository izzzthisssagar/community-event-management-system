using CommunityEventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityEventManagement.Infrastructure.Data.Configurations;

/// <summary>
/// UserConfiguration maps the User (login account) entity to the database. The email is unique
/// because users log in with their email address, and the password is always stored as a hash.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName).IsRequired().HasMaxLength(150);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role).IsRequired().HasMaxLength(50);
        builder.Property(u => u.ConcurrencyToken).IsConcurrencyToken();

        // Email must be unique so two accounts can not share the same login.
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
