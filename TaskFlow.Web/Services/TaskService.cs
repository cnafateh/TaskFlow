using Microsoft.EntityFrameworkCore;
using TaskFlow.Web.Data;
using TaskFlow.Web.Models;

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

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _context.Tasks
        .Include(task => task.Project)
        .Include(task => task.Category)
        .AsNoTracking()
        .FirstOrDefaultAsync(task => task.Id == id);
    }

    public async Task CreateAsync(TaskItem task)
    {
        _context.Tasks.Add(task);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(TaskItem task)
    {
        TaskItem? existingTask =
            await _context.Tasks.FindAsync(task.Id);

        if (existingTask == null)
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

    public async Task<bool> DeleteAsync(int id)
    {
        TaskItem? task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return false;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<TaskItem>> GetFilteredAsync(TaskFilter filter)
    {
        IQueryable<TaskItem> query = _context.Tasks
            .Include(task => task.Project)
            .Include(task => task.Category)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            string search = filter.Search.Trim();

            query = query.Where(task =>
                task.Title.Contains(search) ||
                task.Description.Contains(search));
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

            "dueDateDesc" =>
                query.OrderByDescending(task => task.DueDate),

            "createdAt" =>
                query.OrderBy(task => task.CreatedAt),

            "createdAtDesc" =>
                query.OrderByDescending(task => task.CreatedAt),

            _ =>
                query.OrderBy(task => task.DueDate)
        };

        return await query.ToListAsync();
    }
}