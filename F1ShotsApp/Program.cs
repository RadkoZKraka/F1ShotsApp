using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DatabaseSetupLocal.Data;
using DatabaseSetupLocal.Areas.Identity.Data;
using DatabaseSetupLocal.Library;
using DatabaseSetupLocal.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<UsersContext>();
builder.Services.AddDbContext<ShotsContext>();
builder.Services.AddTransient<ShotsRepository>();
builder.Services.AddTransient<UserRepository>();
builder.Services.AddHostedService<BackupService>();
builder.Services.AddTransient<LoggingService>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


AppSetup.SeedDb();
int[] years = {2022, 2023};
foreach (var year in years)
{
    AppSetup.SerializeDrivers(year);

}
AppSetup.SerializeDates();
F1WebScraper.GetPoleSitter(2023, 3);
// AppSetup.LockPreviousRaces();
await AppSetup.ScheduleTasks();
// AppSetup.GetCurrentRace();


builder.Services.AddDefaultIdentity<AppUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 7;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<UsersContext>();
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    // options.Password.RequireDigit = true;
    // options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});
builder.Services.AddControllers(config =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    config.Filters.Add(new AuthorizeFilter(policy));
});





builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);

    options.LoginPath = "/Identity/Account/Logins";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});


builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var f1ShotsUserContext = services.GetRequiredService<UsersContext>();
    // f1ShotsUserContext.Database.EnsureDeleted();
    f1ShotsUserContext.Database.Migrate();
    // var shotsContext = services.GetRequiredService<ShotsContext>();
    // shotsContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();