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
}