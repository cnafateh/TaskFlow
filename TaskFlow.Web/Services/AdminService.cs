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

    public async Task<PagedResult<TaskItem>> GetTasksAsync(
    AdminTaskFilter filter)
    {
        IQueryable<TaskItem> query =
            _context.Tasks
                .Include(task => task.Project)
                    .ThenInclude(project => project.User)
                .Include(task => task.Category)
                .AsNoTracking();


        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            string search = filter.Search.Trim();

            query = query.Where(task =>
                task.Title.Contains(search) ||
                task.Description.Contains(search) ||
                task.Project.Name.Contains(search) ||
                task.Category.Name.Contains(search) ||
                (
                    task.Project.User != null &&
                    task.Project.User.Email != null &&
                    task.Project.User.Email.Contains(search)
                ));
        }


        if (!string.IsNullOrWhiteSpace(filter.OwnerId))
        {
            query = query.Where(task =>
                task.Project.UserId == filter.OwnerId);
        }


        if (filter.ProjectId.HasValue)
        {
            query = query.Where(task =>
                task.ProjectId == filter.ProjectId.Value);
        }


        if (filter.CategoryId.HasValue)
        {
            query = query.Where(task =>
                task.CategoryId == filter.CategoryId.Value);
        }


        if (filter.Priority.HasValue)
        {
            query = query.Where(task =>
                task.Priority == filter.Priority.Value);
        }


        if (filter.Status.HasValue)
        {
            query = query.Where(task =>
                task.Status == filter.Status.Value);
        }


        query = filter.SortBy switch
        {
            "title" =>
                query.OrderBy(task => task.Title),

            "titleDesc" =>
                query.OrderByDescending(task => task.Title),

            "dueDate" =>
                query.OrderBy(task => task.DueDate),

            "dueDateDesc" =>
                query.OrderByDescending(task => task.DueDate),

            "createdAt" =>
                query.OrderBy(task => task.CreatedAt),

            "createdAtDesc" =>
                query.OrderByDescending(task => task.CreatedAt),

            _ =>
                query.OrderByDescending(task => task.CreatedAt)
        };


        int totalCount =
            await query.CountAsync();


        if (filter.Page < 1)
        {
            filter.Page = 1;
        }


        int totalPages =
            (int)Math.Ceiling(
                (double)totalCount /
                filter.PageSize);


        if (totalPages > 0 &&
            filter.Page > totalPages)
        {
            filter.Page = totalPages;
        }


        List<TaskItem> tasks =
            await query
                .Skip(
                    (filter.Page - 1) *
                    filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();


        return new PagedResult<TaskItem>
        {
            Items = tasks,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }


    public async Task<List<ApplicationUser>>
        GetTaskOwnersAsync()
    {
        return await _userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .ToListAsync();
    }


    public async Task<List<Project>>
        GetAllProjectsAsync()
    {
        return await _context.Project
            .Include(project => project.User)
            .AsNoTracking()
            .OrderBy(project => project.Name)
            .ToListAsync();
    }


    public async Task<List<Category>>
        GetAllCategoriesAsync()
    {
        return await _context.Categories
            .Include(category => category.User)
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync();
    }


    public async Task<bool> DeleteTaskAsync(
        int id)
    {
        TaskItem? task =
            await _context.Tasks
                .FirstOrDefaultAsync(
                    task => task.Id == id);

        if (task == null)
        {
            return false;
        }


        _context.Tasks.Remove(task);

        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<int> DeleteTasksAsync(
        IEnumerable<int> ids)
    {
        List<int> taskIds =
            ids
                .Distinct()
                .ToList();


        if (taskIds.Count == 0)
        {
            return 0;
        }


        List<TaskItem> tasks =
            await _context.Tasks
                .Where(task =>
                    taskIds.Contains(task.Id))
                .ToListAsync();


        if (tasks.Count == 0)
        {
            return 0;
        }


        _context.Tasks.RemoveRange(tasks);

        await _context.SaveChangesAsync();

        return tasks.Count;
    }
}