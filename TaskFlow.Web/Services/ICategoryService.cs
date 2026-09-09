using TaskFlow.Web.Models;

namespace TaskFlow.Web.Services;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync(string userId);

    Task<Category?> GetByIdAsync(
        int id,
        string userId);

    Task CreateAsync(Category category);

    Task<bool> UpdateAsync(
        Category category,
        string userId);

    Task<bool> DeleteAsync(
        int id,
        string userId);

    Task<bool> HasTasksAsync(
        int categoryId,
        string userId);
}