using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommunityEventManagement.Infrastructure.Data;

/// <summary>
/// This factory is ONLY used by the Entity Framework Core command line tools, for example when I
/// run "dotnet ef migrations add" or "dotnet ef database update". It is NOT used when the actual
/// application runs (the app builds its own DbContext over in Program.cs).
/// I created this class so the tools can build my DbContext without needing the database to be
/// switched on just to detect its version. I simply tell it the exact MariaDB version that my
/// XAMPP installation uses (10.4.32), which makes creating migrations quick and reliable.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // The same connection string the app uses (XAMPP MySQL, database "community_events").
        string sConnectionString = "server=localhost;port=3306;database=community_events;user=root;password=";

        // A fixed MariaDB version so the tools do not have to connect just to work out the version.
        MariaDbServerVersion serverVersion = new MariaDbServerVersion(new Version(10, 4, 32));

        DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseMySql(sConnectionString, serverVersion);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
