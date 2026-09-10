using Microsoft.AspNetCore.Identity;
using TaskFlow.Web.Models;

namespace TaskFlow.Web.Data;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(
        RoleManager<IdentityRole> roleManager)
    {
        string[] roles =
        {
            "Admin",
            "User"
        };

        foreach (string role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(role));
            }
        }
    }


    public static async Task SeedAdminUserAsync(
        UserManager<ApplicationUser> userManager)
    {
        string email = "admin@taskflow.com";

        ApplicationUser? user =
            await userManager.FindByEmailAsync(email);


        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };


            await userManager.CreateAsync(
                user,
                "Admin123!");
        }


        if (!await userManager.IsInRoleAsync(
            user,
            "Admin"))
        {
            await userManager.AddToRoleAsync(
                user,
                "Admin");
        }
    }
}