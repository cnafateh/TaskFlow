using Microsoft.EntityFrameworkCore;
using TaskFlow.Web.Data;
using TaskFlow.Web.Services;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    IServiceProvider services =
        scope.ServiceProvider;


    RoleManager<IdentityRole> roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();


    UserManager<ApplicationUser> userManager =
        services.GetRequiredService<UserManager<ApplicationUser>>();


    await IdentitySeeder.SeedRolesAsync(roleManager);

    await IdentitySeeder.SeedAdminUserAsync(userManager);
}

app.Run();
