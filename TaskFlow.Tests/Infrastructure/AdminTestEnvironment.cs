using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Web.Data;
using TaskFlow.Web.Models;
using TaskFlow.Web.Services;

namespace TaskFlow.Tests.Infrastructure;

public sealed class AdminTestEnvironment : IAsyncDisposable
{
    public SqliteConnection Connection { get; }

    public ServiceProvider Services { get; }

    public AppDbContext Context { get; }

    public UserManager<ApplicationUser> UserManager { get; }

    public AdminService AdminService { get; }


    private AdminTestEnvironment(
        SqliteConnection connection,
        ServiceProvider services,
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        AdminService adminService)
    {
        Connection = connection;
        Services = services;
        Context = context;
        UserManager = userManager;
        AdminService = adminService;
    }


    public static async Task<AdminTestEnvironment> CreateAsync()
    {
        SqliteConnection connection =
            new("Data Source=:memory:");

        await connection.OpenAsync();


        ServiceCollection services = new();

        services.AddLogging();


        services.AddDbContext<AppDbContext>(
            options =>
                options.UseSqlite(connection));


        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();


        ServiceProvider serviceProvider =
            services.BuildServiceProvider();


        AppDbContext context =
            serviceProvider
                .GetRequiredService<AppDbContext>();


        await context.Database.EnsureCreatedAsync();


        RoleManager<IdentityRole> roleManager =
            serviceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();


        foreach (string roleName in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                IdentityResult result =
                    await roleManager.CreateAsync(
                        new IdentityRole(roleName));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not create test role: {roleName}");
                }
            }
        }


        UserManager<ApplicationUser> userManager =
            serviceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();


        AdminService adminService =
            new(
                context,
                userManager);


        return new AdminTestEnvironment(
            connection,
            serviceProvider,
            context,
            userManager,
            adminService);
    }


    public async ValueTask DisposeAsync()
    {
        await Services.DisposeAsync();
        await Connection.DisposeAsync();
    }
}