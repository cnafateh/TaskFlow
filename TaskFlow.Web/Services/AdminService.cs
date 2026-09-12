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

    public async Task<PagedResult<AdminProjectRowViewModel>>
    GetProjectsAsync(
        AdminProjectFilter filter)
    {
        IQueryable<Project> query =
            _context.Project
                .Include(project => project.User)
                .AsNoTracking();


        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            string search =
                filter.Search.Trim();

            query = query.Where(project =>
                project.Name.Contains(search) ||
                (
                    project.Description != null &&
                    project.Description.Contains(search)
                ) ||
                (
                    project.User != null &&
                    project.User.Email != null &&
                    project.User.Email.Contains(search)
                ));
        }


        if (!string.IsNullOrWhiteSpace(filter.OwnerId))
        {
            query = query.Where(project =>
                project.UserId == filter.OwnerId);
        }


        query = filter.SortBy switch
        {
            "name" =>
                query.OrderBy(project =>
                    project.Name),

            "nameDesc" =>
                query.OrderByDescending(project =>
                    project.Name),

            "createdAt" =>
                query.OrderBy(project =>
                    project.CreatedAt),

            "createdAtDesc" =>
                query.OrderByDescending(project =>
                    project.CreatedAt),

            _ =>
                query.OrderByDescending(project =>
                    project.CreatedAt)
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


        List<AdminProjectRowViewModel> projects =
            await query
                .Skip(
                    (filter.Page - 1) *
                    filter.PageSize)
                .Take(filter.PageSize)
                .Select(project =>
                    new AdminProjectRowViewModel
                    {
                        Id = project.Id,

                        Name = project.Name,

                        Description =
                            project.Description,

                        OwnerEmail =
                            project.User != null
                                ? project.User.Email ?? ""
                                : "Unknown",

                        TaskCount =
                            project.Tasks.Count,

                        CreatedAt =
                            project.CreatedAt
                    })
                .ToListAsync();


        return new PagedResult<AdminProjectRowViewModel>
        {
            Items = projects,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }
    public async Task<bool> DeleteProjectAsync(
    int id)
    {
        Project? project =
            await _context.Project
                .FirstOrDefaultAsync(
                    project =>
                        project.Id == id);

        if (project == null)
        {
            return false;
        }


        _context.Project.Remove(project);

        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<int> DeleteProjectsAsync(
    IEnumerable<int> ids)
    {
        List<int> projectIds =
            ids
                .Distinct()
                .ToList();


        if (projectIds.Count == 0)
        {
            return 0;
        }


        List<Project> projects =
            await _context.Project
                .Where(project =>
                    projectIds.Contains(project.Id))
                .ToListAsync();


        if (projects.Count == 0)
        {
            return 0;
        }


        _context.Project.RemoveRange(
            projects);

        await _context.SaveChangesAsync();

        return projects.Count;
    }


    public async Task<PagedResult<AdminCategoryRowViewModel>>
    GetCategoriesAsync(
        AdminCategoryFilter filter)
    {
        IQueryable<Category> query =
            _context.Categories
                .Include(category =>
                    category.User)
                .AsNoTracking();


        if (!string.IsNullOrWhiteSpace(
                filter.Search))
        {
            string search =
                filter.Search.Trim();

            query = query.Where(category =>
                category.Name.Contains(search) ||
                (
                    category.User != null &&
                    category.User.Email != null &&
                    category.User.Email.Contains(search)
                ));
        }


        if (!string.IsNullOrWhiteSpace(
                filter.OwnerId))
        {
            query = query.Where(category =>
                category.UserId ==
                filter.OwnerId);
        }


        query = filter.SortBy switch
        {
            "nameDesc" =>
                query.OrderByDescending(
                    category =>
                        category.Name),

            _ =>
                query.OrderBy(
                    category =>
                        category.Name)
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


        List<AdminCategoryRowViewModel> categories =
            await query
                .Skip(
                    (filter.Page - 1) *
                    filter.PageSize)
                .Take(filter.PageSize)
                .Select(category =>
                    new AdminCategoryRowViewModel
                    {
                        Id =
                            category.Id,

                        Name =
                            category.Name,

                        OwnerEmail =
                            category.User != null
                                ? category.User.Email ?? ""
                                : "Unknown",

                        TaskCount =
                            category.Tasks.Count
                    })
                .ToListAsync();


        return new PagedResult<AdminCategoryRowViewModel>
        {
            Items = categories,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<bool> DeleteCategoryAsync(
    int id)
    {
        Category? category =
            await _context.Categories
                .FirstOrDefaultAsync(
                    category =>
                        category.Id == id);

        if (category == null)
        {
            return false;
        }


        bool hasTasks =
            await _context.Tasks
                .AnyAsync(task =>
                    task.CategoryId == id);

        if (hasTasks)
        {
            return false;
        }


        _context.Categories.Remove(
            category);

        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<AdminBulkDeleteResult>
    DeleteCategoriesAsync(
        IEnumerable<int> ids)
    {
        List<int> categoryIds =
            ids
                .Distinct()
                .ToList();


        if (categoryIds.Count == 0)
        {
            return new AdminBulkDeleteResult();
        }


        List<Category> categories =
            await _context.Categories
                .Where(category =>
                    categoryIds.Contains(
                        category.Id))
                .ToListAsync();


        List<int> categoriesWithTasks =
            await _context.Tasks
                .Where(task =>
                    categoryIds.Contains(
                        task.CategoryId))
                .Select(task =>
                    task.CategoryId)
                .Distinct()
                .ToListAsync();


        List<Category> deletable =
            categories
                .Where(category =>
                    !categoriesWithTasks.Contains(
                        category.Id))
                .ToList();


        _context.Categories.RemoveRange(
            deletable);

        await _context.SaveChangesAsync();


        return new AdminBulkDeleteResult
        {
            DeletedCount =
                deletable.Count,

            SkippedCount =
                categories.Count -
                deletable.Count
        };
    }
}