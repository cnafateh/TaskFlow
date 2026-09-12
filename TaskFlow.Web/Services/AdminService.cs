using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Web.Data;
using TaskFlow.Web.Models;
using TaskFlow.Web.Models.Enums;
using TaskFlow.Web.ViewModels.Admin;

namespace TaskFlow.Web.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(
        AppDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    public async Task<AdminDashboardViewModel> GetDashboardAsync()
    {
        int totalUsers =
            await _userManager.Users.CountAsync();

        IList<ApplicationUser> admins =
            await _userManager.GetUsersInRoleAsync("Admin");

        int totalProjects =
            await _context.Project.CountAsync();

        int totalTasks =
            await _context.Tasks.CountAsync();

        int completedTasks =
            await _context.Tasks.CountAsync(
                task => task.Status == Models.Enums.TaskStatus.Done);

        return new AdminDashboardViewModel
        {
            TotalUsers = totalUsers,
            TotalAdmins = admins.Count,
            TotalProjects = totalProjects,
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks
        };
    }


    public async Task<List<AdminUserViewModel>> GetUsersAsync()
    {
        List<ApplicationUser> users =
            await _userManager.Users
                .AsNoTracking()
                .ToListAsync();

        List<AdminUserViewModel> result = new();

        foreach (ApplicationUser user in users)
        {
            bool isAdmin =
                await _userManager.IsInRoleAsync(
                    user,
                    "Admin");

            result.Add(new AdminUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                Role = isAdmin
                    ? "Admin"
                    : "User"
            });
        }

        return result;
    }


    public async Task<string?> UpdateUserRoleAsync(
        string userId,
        string role,
        string currentAdminId)
    {
        if (role != "Admin" &&
            role != "User")
        {
            return "Invalid role.";
        }


        ApplicationUser? user =
            await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return "User not found.";
        }


        if (user.Id == currentAdminId)
        {
            return "You cannot change your own role.";
        }


        bool isCurrentlyAdmin =
            await _userManager.IsInRoleAsync(
                user,
                "Admin");


        if (isCurrentlyAdmin &&
            role == "User")
        {
            IList<ApplicationUser> admins =
                await _userManager
                    .GetUsersInRoleAsync("Admin");

            if (admins.Count <= 1)
            {
                return "The last administrator cannot be removed.";
            }
        }


        IList<string> currentRoles =
            await _userManager.GetRolesAsync(user);


        if (currentRoles.Count > 0)
        {
            IdentityResult removeResult =
                await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);

            if (!removeResult.Succeeded)
            {
                return "Could not remove the current role.";
            }
        }


        IdentityResult addResult =
            await _userManager.AddToRoleAsync(
                user,
                role);

        if (!addResult.Succeeded)
        {
            return "Could not assign the new role.";
        }


        return null;
    }
}