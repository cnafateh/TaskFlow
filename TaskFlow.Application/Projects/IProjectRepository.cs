using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Projects;

public interface IProjectRepository
{
    Task<List<Project>> GetAllForUserAsync(
        string userId);

    Task<Project?> GetByIdForUserAsync(
        int id,
        string userId);

    Task<Project?> GetTrackedByIdForUserAsync(
        int id,
        string userId);

    Task<bool> ExistsForUserAsync(
        int id,
        string userId);

    Task AddAsync(Project project);

    void Remove(Project project);

    Task SaveChangesAsync();
}