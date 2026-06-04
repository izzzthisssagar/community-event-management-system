using CommunityEventManagement.Components;
using CommunityEventManagement.Infrastructure.Data;
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
