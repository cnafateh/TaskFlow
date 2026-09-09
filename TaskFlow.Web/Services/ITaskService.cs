using TaskFlow.Web.Models;

namespace TaskFlow.Web.Services;

public interface ITaskService
{
    Task<List<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task CreateAsync(TaskItem task);
    Task<bool> UpdateAsync(TaskItem task);
    Task<bool> DeleteAsync(int id);
    Task<PagedResult<TaskItem>> GetFilteredAsync(TaskFilter filter);
}