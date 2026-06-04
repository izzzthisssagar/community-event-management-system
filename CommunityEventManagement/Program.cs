using CommunityEventManagement.Application.Services;
using CommunityEventManagement.Components;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Infrastructure.Data;
using CommunityEventManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Read the MySQL connection string from appsettings.json.
string sConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("The 'DefaultConnection' connection string was not found.");

// I register the DbContext using AddDbContextFactory (NOT AddDbContext). This is very important
// for Blazor Server: a single circuit can stay open for a long time and render several
// components at once, and they would all fight over one shared DbContext and crash. The factory
// instead hands out a brand new, short-lived DbContext for each database operation, which is the
// safe pattern for Blazor Server.
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseMySql(sConnectionString, ServerVersion.AutoDetect(sConnectionString)));

// ----- Repositories -----
// I register each repository against its interface. The lifetime is Scoped, which in Blazor
// Server means one instance per circuit (per user connection). The repositories still create a
// fresh DbContext for every call through the factory, so this stays safe.
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
builder.Services.AddScoped<IVenueRepository, VenueRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();

// ----- Services -----
// The services hold the business logic and depend on the repository interfaces above. Registering
// them by their interface is what lets dependency injection wire everything together for me.
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
