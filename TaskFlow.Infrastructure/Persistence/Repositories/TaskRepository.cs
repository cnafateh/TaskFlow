using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common;
using TaskFlow.Application.Tasks;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<TaskItem?> GetByIdForUserAsync(
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


    public async Task<TaskItem?> GetTrackedByIdForUserAsync(
        int id,
        string userId)
    {
        return await _context.Tasks
            .FirstOrDefaultAsync(task =>
                task.Id == id &&
                task.Project.UserId == userId);
    }


    public async Task AddAsync(TaskItem task)
    {
        await _context.Tasks.AddAsync(task);
    }


    public void Remove(TaskItem task)
    {
        _context.Tasks.Remove(task);
    }


    public async Task<PagedResult<TaskItem>> GetFilteredAsync(
        TaskFilter filter,
        string userId)
    {
        IQueryable<TaskItem> query =
            _context.Tasks
                .Include(task => task.Project)
                .Include(task => task.Category)
                .Where(task =>
                    task.Project.UserId == userId)
                .AsNoTracking();


        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            string search =
                filter.Search.Trim();

            query = query.Where(task =>
                task.Title.Contains(search) ||
                task.Description.Contains(search));
        }


        if (filter.ProjectId.HasValue)
        {
            query = query.Where(task =>
                task.ProjectId ==
                filter.ProjectId.Value);
        }


        if (filter.CategoryId.HasValue)
        {
            query = query.Where(task =>
                task.CategoryId ==
                filter.CategoryId.Value);
        }


        if (filter.Priority.HasValue)
        {
            query = query.Where(task =>
                task.Priority ==
                filter.Priority.Value);
        }


        if (filter.Status.HasValue)
        {
            query = query.Where(task =>
                task.Status ==
                filter.Status.Value);
        }


        query = filter.SortBy switch
        {
            "title" =>
                query.OrderBy(task =>
                    task.Title),

            "titleDesc" =>
                query.OrderByDescending(task =>
                    task.Title),

            "dueDateDesc" =>
                query.OrderByDescending(task =>
                    task.DueDate),

            "createdAt" =>
                query.OrderBy(task =>
                    task.CreatedAt),

            "createdAtDesc" =>
                query.OrderByDescending(task =>
                    task.CreatedAt),

            _ =>
                query.OrderBy(task =>
                    task.DueDate)
        };


        int totalCount =
            await query.CountAsync();


        int page =
            filter.Page < 1
                ? 1
                : filter.Page;

        int pageSize =
            filter.PageSize < 1
                ? 10
                : filter.PageSize;


        List<TaskItem> items =
            await query
                .Skip(
                    (page - 1) *
                    pageSize)
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


    public async Task<TaskSummary> GetSummaryAsync(
        string userId)
    {
        IQueryable<TaskItem> query =
            _context.Tasks
                .Where(task =>
                    task.Project.UserId == userId);


        int totalTasks =
            await query.CountAsync();

        int inProgressTasks =
            await query.CountAsync(task =>
                task.Status ==
                TaskFlow.Domain.Enums.TaskStatus.InProgress);

        int completedTasks =
            await query.CountAsync(task =>
                task.Status ==
                TaskFlow.Domain.Enums.TaskStatus.Done);


        return new TaskSummary
        {
            TotalTasks = totalTasks,
            InProgressTasks = inProgressTasks,
            CompletedTasks = completedTasks
        };
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}