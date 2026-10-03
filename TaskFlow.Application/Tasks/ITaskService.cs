using TaskFlow.Application.Common;
using TaskFlow.Domain.Entities;
namespace TaskFlow.Application.Tasks;

public interface ITaskService
{
    Task<TaskItem?> GetByIdAsync(int id, string userId);

    Task<bool> CreateAsync(TaskItem task, string userId);

    Task<bool> UpdateAsync(TaskItem task, string userId);

    Task<bool> DeleteAsync(int id, string userId);

    Task<PagedResult<TaskItem>> GetFilteredAsync(
        TaskFilter filter,
        string userId);

    Task<TaskSummary> GetSummaryAsync(string userId);
}