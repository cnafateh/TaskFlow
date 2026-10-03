using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Categories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllForUserAsync(
        string userId);

    Task<Category?> GetByIdForUserAsync(
        int id,
        string userId);

    Task<Category?> GetTrackedByIdForUserAsync(
        int id,
        string userId);

    Task<bool> ExistsForUserAsync(
        int id,
        string userId);

    Task<bool> HasTasksForUserAsync(
        int categoryId,
        string userId);

    Task AddAsync(Category category);

    void Remove(Category category);

    Task SaveChangesAsync();
}