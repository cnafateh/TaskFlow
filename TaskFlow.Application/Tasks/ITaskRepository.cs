using TaskFlow.Application.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Tasks;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdForUserAsync(
        int id,
        string userId);

    Task<TaskItem?> GetTrackedByIdForUserAsync(
        int id,
        string userId);

    Task AddAsync(TaskItem task);

    void Remove(TaskItem task);

    Task<PagedResult<TaskItem>> GetFilteredAsync(
        TaskFilter filter,
        string userId);

    Task<TaskSummary> GetSummaryAsync(
        string userId);

    Task SaveChangesAsync();
}