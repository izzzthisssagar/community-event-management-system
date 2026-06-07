using CommunityEventManagement.Application.Services;
using CommunityEventManagement.Application.Validators;
using CommunityEventManagement.Components;
using CommunityEventManagement.Domain.Interfaces;
using CommunityEventManagement.Infrastructure.Data;
using CommunityEventManagement.Infrastructure.Repositories;
using CommunityEventManagement.Models.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
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

// The toast service shows small pop-up notifications. It is Scoped so the pages and the Toaster
// component share one instance per user connection.
builder.Services.AddScoped<IToastService, ToastService>();

// The account service lets a visitor create their own account (a User plus a linked Participant).
builder.Services.AddScoped<IAccountService, AccountService>();

// ----- Authentication -----
// The user repository and the auth service handle logging people in and out.
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// MVC controllers are needed for my AuthController (the login form posts to it).
builder.Services.AddControllersWithViews();

// IHttpContextAccessor lets my AuthService reach the current HttpContext so it can write the
// authentication cookie during the login request.
builder.Services.AddHttpContextAccessor();

// Set up cookie authentication. LoginPath is where unauthenticated users get sent.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();

// This makes the signed-in user's identity available to my Blazor components as a cascading value.
builder.Services.AddCascadingAuthenticationState();

// ----- Validation -----
// I register each FluentValidation validator against IValidator<T>. The FluentValidationValidator
// component inside my EditForms then picks the right one up automatically and runs it.
builder.Services.AddScoped<IValidator<EventViewModel>, EventValidator>();
builder.Services.AddScoped<IValidator<ParticipantViewModel>, ParticipantValidator>();
builder.Services.AddScoped<IValidator<VenueViewModel>, VenueValidator>();
builder.Services.AddScoped<IValidator<ActivityViewModel>, ActivityValidator>();
builder.Services.AddScoped<IValidator<LoginViewModel>, LoginViewModelValidator>();
builder.Services.AddScoped<IValidator<SignUpViewModel>, SignUpViewModelValidator>();

var app = builder.Build();

// Seed the database as the application starts. This applies my migrations (creating the database
// and tables if needed) and adds the default admin account plus some sample data. I create a
// scope first because the seeder needs scoped services.
using (IServiceScope scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// The authentication middleware reads the cookie and works out who the user is, and the
// authorization middleware then enforces any [Authorize] rules. They must come before the
// antiforgery and endpoint middleware below.
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

// Map my MVC controllers (this is what makes the /auth/... routes work).
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
