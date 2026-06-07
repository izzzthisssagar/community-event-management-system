using CommunityEventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommunityEventManagement.Infrastructure.Data;

/// <summary>
/// DbSeeder makes sure the database exists, is up to date with my migrations, and has some data in
/// it the first time the application runs. It always creates a default Admin account so I can log
/// in, and it adds some sample venues, activities, participants and events so the app is not empty
/// when it is opened (which also makes the demo video much easier to record).
/// It is written so it is safe to run every time the app starts: it checks first and only adds
/// things that are not already there.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Runs the whole seed process. It is given the service provider so it can get a DbContext from
    /// the factory in the same safe way the rest of the app does.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        IDbContextFactory<ApplicationDbContext> dcfContextFactory =
            serviceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

        using ApplicationDbContext context = await dcfContextFactory.CreateDbContextAsync();

        // Apply any pending migrations. This creates the database and all of the tables if they do
        // not exist yet, so I never have to create them by hand.
        await context.Database.MigrateAsync();

        // Seed the accounts and the sample data, then save once.
        await SeedAdminUserAsync(context);
        await SeedDemoUserAsync(context);
        await SeedSampleDataAsync(context);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Adds the default administrator account, but only if no admin exists yet. The password is
    /// hashed with BCrypt before it is stored — the plain password is never saved.
    /// </summary>
    private static async Task SeedAdminUserAsync(ApplicationDbContext context)
    {
        bool bAdminExists = await context.Users.AnyAsync(u => u.Role == "Admin");
        if (bAdminExists)
        {
            return;
        }

        string sHashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        User adminUser = new User("System Administrator", "admin@events.com", sHashedPassword, "Admin");
        context.Users.Add(adminUser);
    }

    /// <summary>
    /// Adds a normal (non-admin) demo user so the User role can be shown off. The account is linked
    /// to a participant profile by sharing the same email, which is what lets this user register
    /// themselves for events and see their own registrations.
    /// </summary>
    private static async Task SeedDemoUserAsync(ApplicationDbContext context)
    {
        const string sEmail = "user@events.com";

        bool bUserExists = await context.Users.AnyAsync(u => u.Email == sEmail);
        if (bUserExists)
        {
            return;
        }

        // Make sure a matching participant profile exists (same email links the two together).
        bool bParticipantExists = await context.Participants.AnyAsync(p => p.Email == sEmail);
        if (!bParticipantExists)
        {
            context.Participants.Add(new Participant("Demo", "User", sEmail, "07000000099"));
        }

        string sHashedPassword = BCrypt.Net.BCrypt.HashPassword("User123!");
        context.Users.Add(new User("Demo User", sEmail, sHashedPassword, "User"));
    }

    /// <summary>
    /// Adds some sample venues, activities, participants and events, but only if there are no
    /// events yet. Everything is created in memory and linked together first, then EF Core works
    /// out all of the inserts (including the many-to-many join rows) when the changes are saved.
    /// </summary>
    private static async Task SeedSampleDataAsync(ApplicationDbContext context)
    {
        bool bEventsExist = await context.Events.AnyAsync();
        if (bEventsExist)
        {
            return;
        }

        // ----- Venues -----
        Venue communityHall = new Venue("Community Hall", "123 Main Street", 200, true);
        Venue sportsField = new Venue("Riverside Sports Field", "456 Park Avenue", 500, false);
        Venue conferenceCentre = new Venue("City Conference Centre", "789 Business Road", 150, true);
        context.Venues.AddRange(communityHall, sportsField, conferenceCentre);

        // ----- Activities (one of each subclass, plus an extra workshop) -----
        WorkshopActivity potteryWorkshop = new WorkshopActivity("Pottery Workshop", 90, "Jane Smith", "Clay and tools provided");
        WorkshopActivity codingBootcamp = new WorkshopActivity("Coding Bootcamp", 120, "Alan Turing", "Please bring a laptop");
        GameActivity communityFootball = new GameActivity("Community Football", 60, 12, true);
        TalkActivity climateTalk = new TalkActivity("Climate Action Talk", 45, "Dr. Green", "Sustainability");
        context.Activities.AddRange(potteryWorkshop, codingBootcamp, communityFootball, climateTalk);

        // ----- Participants -----
        Participant alice = new Participant("Alice", "Johnson", "alice.johnson@example.com", "07000000001");
        Participant bob = new Participant("Bob", "Williams", "bob.williams@example.com", "07000000002");
        Participant chloe = new Participant("Chloe", "Brown", "chloe.brown@example.com", "07000000003");
        context.Participants.AddRange(alice, bob, chloe);

        // ----- Events (linked to the venues and activities created above) -----
        Event summerFair = new Event(
            "Summer Community Fair",
            DateTime.Today.AddDays(14),
            new TimeSpan(10, 0, 0),
            new TimeSpan(16, 0, 0),
            "A fun day out for the whole community with stalls, food and activities.",
            200);
        summerFair.AddVenue(communityHall);
        summerFair.AddActivity(potteryWorkshop);
        summerFair.AddActivity(communityFootball);

        Event techDay = new Event(
            "Community Tech Day",
            DateTime.Today.AddDays(21),
            new TimeSpan(9, 30, 0),
            new TimeSpan(15, 0, 0),
            "Hands-on technology sessions and talks for all ages and abilities.",
            150);
        techDay.AddVenue(conferenceCentre);
        techDay.AddActivity(codingBootcamp);
        techDay.AddActivity(climateTalk);

        Event sportsDay = new Event(
            "Charity Sports Day",
            DateTime.Today.AddDays(30),
            new TimeSpan(11, 0, 0),
            new TimeSpan(17, 0, 0),
            "A friendly sports day raising money for local good causes.",
            500);
        sportsDay.AddVenue(sportsField);
        sportsDay.AddActivity(communityFootball);

        context.Events.AddRange(summerFair, techDay, sportsDay);
    }
}
