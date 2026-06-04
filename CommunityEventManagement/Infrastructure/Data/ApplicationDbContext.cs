using CommunityEventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommunityEventManagement.Infrastructure.Data;

/// <summary>
/// ApplicationDbContext is the main Entity Framework Core class that represents my database.
/// Each DbSet below becomes a table in MySQL. EF Core uses this class to translate my C# code
/// (LINQ queries, saves, etc.) into the real SQL that runs against the database.
/// I deliberately gave it a constructor that takes DbContextOptions so the same context can be
/// used both with MySQL (when the app runs) and with SQLite in-memory (when my unit tests run).
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// The options (which database provider, which connection string, etc.) are passed in from
    /// the outside. This is dependency injection — the context does not hard-code the database.
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Each DbSet is one table. Note there is only ONE DbSet for Activity even though I have three
    // subclasses — that is because I use Table-Per-Hierarchy, so all three activity types live in
    // the same table and EF Core tells them apart using a discriminator column.
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// OnModelCreating is where the database model is built. Instead of writing all of the
    /// configuration here in one giant method, I keep each entity's configuration in its own
    /// class (in the Configurations folder). This line finds all of those classes automatically
    /// and applies them, which keeps this method clean and easy to read.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply every IEntityTypeConfiguration<T> found in this project's assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
