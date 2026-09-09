using TaskFlow.Web.Models;

namespace TaskFlow.Web.Services;

public interface ITaskService
{
    Task<TaskItem?> GetByIdAsync(int id, string userId);

    Task<bool> CreateAsync(TaskItem task, string userId);

    Task<bool> UpdateAsync(TaskItem task, string userId);

    Task<bool> DeleteAsync(int id, string userId);

    Task<PagedResult<TaskItem>> GetFilteredAsync(
        TaskFilter filter,
        string userId);
}