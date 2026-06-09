using CommunityEventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommunityEventManagement.Infrastructure.Data;

/// <summary>
/// DbSeeder makes sure the database exists, is up to date with my migrations, and has some data in
/// it the first time the application runs. It always creates a default Admin account so I can log
/// in, and it adds sample venues, activities, participants and events so the app is not empty
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

        // Save accounts first so a failure in sample data never prevents login.
        await SeedAdminUserAsync(context);
        await SeedDemoUserAsync(context);
        await context.SaveChangesAsync();

        // Sample data is saved separately so its failure is isolated.
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
    /// Adds sample venues, activities, participants and events (at least 10 of each), but only if
    /// there are no events yet. Everything is created in memory and linked together first, then
    /// EF Core works out all of the inserts (including the many-to-many join rows) when saved.
    /// </summary>
    private static async Task SeedSampleDataAsync(ApplicationDbContext context)
    {
        bool bEventsExist = await context.Events.AnyAsync();
        if (bEventsExist)
        {
            return;
        }

        // ----- Venues (10) -----
        Venue communityHall      = new Venue("Community Hall",          "123 Main Street",       200,  true);
        Venue sportsField        = new Venue("Riverside Sports Field",  "456 Park Avenue",       500,  false);
        Venue conferenceCentre   = new Venue("City Conference Centre",  "789 Business Road",     150,  true);
        Venue townLibrary        = new Venue("Town Library",            "12 Library Lane",       80,   true);
        Venue rooftopGarden      = new Venue("Rooftop Garden",          "45 High Street",        60,   false);
        Venue sportsArena        = new Venue("Indoor Sports Arena",     "88 Arena Way",          800,  true);
        Venue communityKitchen   = new Venue("Community Kitchen",       "5 Cook Street",         30,   true);
        Venue openAirTheatre     = new Venue("Open Air Theatre",        "99 Park Road",          300,  false);
        Venue innovationHub      = new Venue("Innovation Hub",          "22 Tech Boulevard",     120,  true);
        Venue villageGreen       = new Venue("Village Green",           "1 Green Lane",          1000, false);
        context.Venues.AddRange(
            communityHall, sportsField, conferenceCentre, townLibrary, rooftopGarden,
            sportsArena, communityKitchen, openAirTheatre, innovationHub, villageGreen);

        // ----- Activities (11 — mix of all three subclasses) -----
        WorkshopActivity potteryWorkshop    = new WorkshopActivity("Pottery Workshop",         90,  "Jane Smith",      "Clay and tools provided");
        WorkshopActivity codingBootcamp     = new WorkshopActivity("Coding Bootcamp",          120, "Alan Turing",     "Please bring a laptop");
        WorkshopActivity cookingMasterclass = new WorkshopActivity("Cooking Masterclass",      60,  "Chef Marco",      "All ingredients provided");
        WorkshopActivity watercolour        = new WorkshopActivity("Watercolour Painting",     75,  "Emma Clarke",     "Art supplies included");
        WorkshopActivity firstAid           = new WorkshopActivity("First Aid Training",       120, "Dr. Sarah Patel", "Training kits provided");
        GameActivity     communityFootball  = new GameActivity("Community Football",           60,  12,  true);
        GameActivity     boardGameMarathon  = new GameActivity("Board Game Marathon",          180, 8,   true);
        GameActivity     outdoorVolleyball  = new GameActivity("Outdoor Volleyball",           60,  10,  true);
        TalkActivity     climateTalk        = new TalkActivity("Climate Action Talk",          45,  "Dr. Green",         "Sustainability");
        TalkActivity     digitalWellbeing   = new TalkActivity("Digital Wellbeing Talk",       60,  "Prof. James Reid",  "Mental health and technology");
        TalkActivity     historyTour        = new TalkActivity("Local History Presentation",   90,  "Ms. Helen Ward",    "Sunderland heritage");
        context.Activities.AddRange(
            potteryWorkshop, codingBootcamp, cookingMasterclass, watercolour, firstAid,
            communityFootball, boardGameMarathon, outdoorVolleyball,
            climateTalk, digitalWellbeing, historyTour);

        // ----- Participants (10) -----
        Participant alice   = new Participant("Alice",   "Johnson",  "alice.johnson@example.com",  "07000000001");
        Participant bob     = new Participant("Bob",     "Williams", "bob.williams@example.com",   "07000000002");
        Participant chloe   = new Participant("Chloe",   "Brown",    "chloe.brown@example.com",    "07000000003");
        Participant david   = new Participant("David",   "Evans",    "david.evans@example.com",    "07000000004");
        Participant emma    = new Participant("Emma",    "Wilson",   "emma.wilson@example.com",    "07000000005");
        Participant frank   = new Participant("Frank",   "Taylor",   "frank.taylor@example.com",   "07000000006");
        Participant grace   = new Participant("Grace",   "Martin",   "grace.martin@example.com",   "07000000007");
        Participant henry   = new Participant("Henry",   "Thompson", "henry.thompson@example.com", "07000000008");
        Participant isabel  = new Participant("Isabel",  "Garcia",   "isabel.garcia@example.com",  "07000000009");
        Participant jack    = new Participant("Jack",    "Robinson", "jack.robinson@example.com",  "07000000010");
        context.Participants.AddRange(alice, bob, chloe, david, emma, frank, grace, henry, isabel, jack);

        // ----- Events (11, all in the future) -----
        Event summerFair = new Event(
            "Summer Community Fair",
            DateTime.Today.AddDays(14),
            new TimeSpan(10, 0, 0), new TimeSpan(16, 0, 0),
            "A fun day out for the whole community with stalls, food and activities.",
            200);
        summerFair.AddVenue(communityHall);
        summerFair.AddActivity(potteryWorkshop);
        summerFair.AddActivity(communityFootball);

        Event techDay = new Event(
            "Community Tech Day",
            DateTime.Today.AddDays(21),
            new TimeSpan(9, 30, 0), new TimeSpan(15, 0, 0),
            "Hands-on technology sessions and talks for all ages and abilities.",
            150);
        techDay.AddVenue(conferenceCentre);
        techDay.AddActivity(codingBootcamp);
        techDay.AddActivity(digitalWellbeing);

        Event sportsDay = new Event(
            "Charity Sports Day",
            DateTime.Today.AddDays(30),
            new TimeSpan(11, 0, 0), new TimeSpan(17, 0, 0),
            "A friendly sports day raising money for local good causes.",
            500);
        sportsDay.AddVenue(sportsField);
        sportsDay.AddActivity(communityFootball);
        sportsDay.AddActivity(outdoorVolleyball);

        Event cookingEvent = new Event(
            "Cooking for Beginners",
            DateTime.Today.AddDays(7),
            new TimeSpan(14, 0, 0), new TimeSpan(16, 0, 0),
            "Learn essential cooking skills in a relaxed, friendly kitchen environment.",
            30);
        cookingEvent.AddVenue(communityKitchen);
        cookingEvent.AddActivity(cookingMasterclass);

        Event artMorning = new Event(
            "Art and Crafts Morning",
            DateTime.Today.AddDays(10),
            new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0),
            "A relaxed morning of watercolour painting and pottery for all skill levels.",
            80);
        artMorning.AddVenue(townLibrary);
        artMorning.AddActivity(watercolour);
        artMorning.AddActivity(potteryWorkshop);

        Event gamesAfternoon = new Event(
            "Family Games Afternoon",
            DateTime.Today.AddDays(12),
            new TimeSpan(13, 0, 0), new TimeSpan(17, 0, 0),
            "Bring the family and enjoy a wide selection of board games and outdoor activities.",
            300);
        gamesAfternoon.AddVenue(openAirTheatre);
        gamesAfternoon.AddActivity(boardGameMarathon);

        Event greenForum = new Event(
            "Green Future Forum",
            DateTime.Today.AddDays(18),
            new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0),
            "Expert talks and discussions on climate action and sustainable living.",
            120);
        greenForum.AddVenue(innovationHub);
        greenForum.AddActivity(climateTalk);

        Event digitalSkills = new Event(
            "Digital Skills Workshop",
            DateTime.Today.AddDays(25),
            new TimeSpan(10, 0, 0), new TimeSpan(13, 0, 0),
            "Practical coding and digital literacy sessions suitable for beginners.",
            120);
        digitalSkills.AddVenue(innovationHub);
        digitalSkills.AddActivity(codingBootcamp);

        Event heritageWalk = new Event(
            "Heritage Presentation Evening",
            DateTime.Today.AddDays(35),
            new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0),
            "An illustrated talk exploring the history and heritage of the local area.",
            80);
        heritageWalk.AddVenue(townLibrary);
        heritageWalk.AddActivity(historyTour);

        Event fitnessDay = new Event(
            "Community Fitness Day",
            DateTime.Today.AddDays(40),
            new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0),
            "Outdoor fitness activities for all abilities including volleyball and football.",
            800);
        fitnessDay.AddVenue(sportsArena);
        fitnessDay.AddActivity(outdoorVolleyball);
        fitnessDay.AddActivity(communityFootball);

        Event wellbeingEvening = new Event(
            "Wellbeing and Technology Evening",
            DateTime.Today.AddDays(45),
            new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0),
            "An informative evening on managing screen time and protecting your mental health.",
            150);
        wellbeingEvening.AddVenue(conferenceCentre);
        wellbeingEvening.AddActivity(digitalWellbeing);

        context.Events.AddRange(
            summerFair, techDay, sportsDay, cookingEvent, artMorning,
            gamesAfternoon, greenForum, digitalSkills, heritageWalk,
            fitnessDay, wellbeingEvening);
    }
}
