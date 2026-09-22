using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Web.Data;
using TaskFlow.Web.Filters;
using TaskFlow.Web.Models;
using TaskFlow.Web.Services;

var builder = WebApplication.CreateBuilder(args);


// MVC + Razor Pages
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<ActionLoggingFilter>();
});
builder.Services.AddRazorPages();


// Application services
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ActionLoggingFilter>();


// Database
string? connectionString =
    builder.Configuration
        .GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite(connectionString);
    }
    else{ 
        
        options.UseNpgsql(connectionString);
    }
});


// Identity
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath =
        "/Home/AccessDenied";
});


// Persist Data Protection keys when a path
// is supplied by the deployment environment.
string? dataProtectionKeysPath =
    builder.Configuration[
        "DataProtection:KeysPath"];

if (!string.IsNullOrWhiteSpace(
        dataProtectionKeysPath))
{
    builder.Services
        .AddDataProtection()
        .PersistKeysToFileSystem(
            new DirectoryInfo(
                dataProtectionKeysPath))
        .SetApplicationName("TaskFlow");
}


// Basic container / deployment health endpoint
builder.Services.AddHealthChecks();


var app = builder.Build();


// Production exception handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();


app.UseStatusCodePagesWithReExecute(
    "/Home/StatusCodePage",
    "?code={0}");


app.MapControllerRoute(
        name: "default",
        pattern:
            "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.MapHealthChecks("/health");


// Database migration + Identity seed
using (IServiceScope scope =
       app.Services.CreateScope())
{
    IServiceProvider services =
        scope.ServiceProvider;


    AppDbContext dbContext =
        services.GetRequiredService<
            AppDbContext>();

    if (app.Environment.IsDevelopment())
    {
        await dbContext.Database
            .EnsureCreatedAsync();
    }
    else
    {
        await dbContext.Database
            .MigrateAsync();
    }


    RoleManager<IdentityRole> roleManager =
        services.GetRequiredService<
            RoleManager<IdentityRole>>();


    UserManager<ApplicationUser> userManager =
        services.GetRequiredService<
            UserManager<ApplicationUser>>();


    await IdentitySeeder
        .SeedRolesAsync(roleManager);


    await IdentitySeeder
        .SeedAdminUserAsync(
            userManager,
            builder.Configuration);
}


app.Run();