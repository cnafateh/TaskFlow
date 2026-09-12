using Microsoft.EntityFrameworkCore;
using TaskFlow.Web.Data;
using TaskFlow.Web.Models;
using TaskFlow.Web.ViewModels.Tasks;

namespace TaskFlow.Web.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _context.Tasks
        .Include(task => task.Project)
        .Include(task => task.Category)
        .AsNoTracking()
        .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(
    int id,
    string userId)
    {
        return await _context.Tasks
            .Include(task => task.Project)
            .Include(task => task.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(task =>
                task.Id == id &&
                task.Project.UserId == userId);
    }

    public async Task<bool> CreateAsync(
    TaskItem task,
    string userId)
    {
        bool projectExists =
            await _context.Project
                .AnyAsync(project =>
                    project.Id == task.ProjectId &&
                    project.UserId == userId);

        if (!projectExists)
        {
            return false;
        }

        bool categoryExists =
            await _context.Categories
                .AnyAsync(category =>
                    category.Id == task.CategoryId &&
                    category.UserId == userId);

        if (!categoryExists)
        {
            return false;
        }

        _context.Tasks.Add(task);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAsync(
    TaskItem task,
    string userId)
    {
        TaskItem? existingTask =
            await _context.Tasks
                .FirstOrDefaultAsync(existing =>
                    existing.Id == task.Id &&
                    existing.Project.UserId == userId);

        if (existingTask == null)
        {
            return false;
        }


        bool projectExists =
            await _context.Project
                .AnyAsync(project =>
                    project.Id == task.ProjectId &&
                    project.UserId == userId);

        if (!projectExists)
        {
            return false;
        }

        bool categoryExists =
            await _context.Categories
            .AnyAsync(category =>
            category.Id == task.CategoryId &&
            category.UserId == userId);

        if (!categoryExists)
        {
            return false;
        }

        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.DueDate = task.DueDate;

        existingTask.ProjectId = task.ProjectId;
        existingTask.CategoryId = task.CategoryId;

        existingTask.Priority = task.Priority;
        existingTask.Status = task.Status;

        existingTask.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(
    int id,
    string userId)
    {
        TaskItem? task =
            await _context.Tasks
                .FirstOrDefaultAsync(task =>
                    task.Id == id &&
                    task.Project.UserId == userId);

        if (task == null)
        {
            return false;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PagedResult<TaskItem>> GetFilteredAsync(
    TaskFilter filter,
    string userId)
    {
        IQueryable<TaskItem> query = _context.Tasks
            .Include(task => task.Project)
            .Include(task => task.Category)
            .Where(task => task.Project.UserId == userId)
            .AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            string search = filter.Search.Trim();

            query = query.Where(task =>
                task.Title.Contains(search) ||
                task.Description.Contains(search));
        }

        // Project filter
        if (filter.ProjectId.HasValue)
        {
            query = query.Where(task =>
                task.ProjectId == filter.ProjectId.Value);
        }

        // Category filter
        if (filter.CategoryId.HasValue)
        {
            query = query.Where(task =>
                task.CategoryId == filter.CategoryId.Value);
        }

        // Priority filter
        if (filter.Priority.HasValue)
        {
            query = query.Where(task =>
                task.Priority == filter.Priority.Value);
        }

        // Status filter
        if (filter.Status.HasValue)
        {
            query = query.Where(task =>
                task.Status == filter.Status.Value);
        }

        // Sorting
        query = filter.SortBy switch
        {
            "title" =>
                query.OrderBy(task => task.Title),

            "titleDesc" =>
                query.OrderByDescending(task => task.Title),

            "dueDateDesc" =>
                query.OrderByDescending(task => task.DueDate),

            "createdAt" =>
                query.OrderBy(task => task.CreatedAt),

            "createdAtDesc" =>
                query.OrderByDescending(task => task.CreatedAt),

            _ =>
                query.OrderBy(task => task.DueDate)
        };

        // تعداد کل Taskها بعد از Filter
        int totalCount = await query.CountAsync();

        int page = filter.Page < 1
            ? 1
            : filter.Page;

        int pageSize = filter.PageSize < 1
            ? 10
            : filter.PageSize;

        // فقط Taskهای صفحه فعلی
        List<TaskItem> items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<TaskItem>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TaskSummaryViewModel> GetSummaryAsync(
    string userId)
    {
        IQueryable<TaskItem> query =
            _context.Tasks
                .Where(task =>
                    task.Project.UserId == userId);

        int totalTasks =
            await query.CountAsync();

        int inProgressTasks =
            await query.CountAsync(
                task =>
                    task.Status == Models.Enums.TaskStatus.InProgress);

        int completedTasks =
            await query.CountAsync(
                task =>
                    task.Status == Models.Enums.TaskStatus.Done);

        return new TaskSummaryViewModel
        {
            TotalTasks = totalTasks,
            InProgressTasks = inProgressTasks,
            CompletedTasks = completedTasks
        };
    }
}