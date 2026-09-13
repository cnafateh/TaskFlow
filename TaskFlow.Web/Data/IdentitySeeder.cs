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
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        string? email =
            configuration["SeedAdmin:Email"];

        string? password =
            configuration["SeedAdmin:Password"];


        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }


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


            IdentityResult createResult =
                await userManager.CreateAsync(
                    user,
                    password);


            if (!createResult.Succeeded)
            {
                string errors =
                    string.Join(
                        "; ",
                        createResult.Errors
                            .Select(error =>
                                error.Description));

                throw new InvalidOperationException(
                    $"Could not create the seeded admin user: {errors}");
            }
        }


        if (!await userManager.IsInRoleAsync(
                user,
                "Admin"))
        {
            IdentityResult roleResult =
                await userManager.AddToRoleAsync(
                    user,
                    "Admin");


            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Could not assign the Admin role to the seeded user.");
            }
        }
    }
}