using CommunityEventManagement.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CommunityEventManagement.Tests.TestHelpers;

/// <summary>
/// TestDbContextFactory gives my repository tests a real, working database to run against, but one
/// that lives only in memory and disappears when the test finishes. I use SQLite in-memory (NOT
/// the EF Core "InMemory" provider) on purpose, because SQLite is a real relational database that
/// actually enforces foreign keys and unique indexes — so my tests check the same rules that MySQL
/// would enforce in production. The EF InMemory provider does not enforce those, which can hide
/// bugs.
/// My repositories depend on IDbContextFactory, so this class implements that same interface. The
/// trick is that all of the contexts it hands out share ONE open SQLite connection, which is what
/// keeps the in-memory database alive and consistent across the whole test.
/// </summary>
public sealed class TestDbContextFactory : IDbContextFactory<ApplicationDbContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public TestDbContextFactory()
    {
        // Open a single in-memory SQLite connection and keep it open for the lifetime of the test.
        // The database exists only while this connection is open.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Build the schema (all of my tables) from the EF model.
        using ApplicationDbContext context = new ApplicationDbContext(_options);
        context.Database.EnsureCreated();
    }

    // Every call returns a fresh context over the same shared connection, exactly like the real
    // factory hands out a fresh context per operation in the running app.
    public ApplicationDbContext CreateDbContext()
    {
        return new ApplicationDbContext(_options);
    }

    public void Dispose()
    {
        // Closing the connection throws away the in-memory database.
        _connection.Dispose();
    }
}
