using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Categories;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;


    public CategoryService(
        ICategoryRepository repository)
    {
        _repository = repository;
    }


    public Task<List<Category>> GetAllAsync(
        string userId)
    {
        return _repository
            .GetAllForUserAsync(userId);
    }


    public Task<Category?> GetByIdAsync(
        int id,
        string userId)
    {
        return _repository
            .GetByIdForUserAsync(
                id,
                userId);
    }


    public async Task CreateAsync(
        Category category)
    {
        await _repository.AddAsync(category);
        await _repository.SaveChangesAsync();
    }


    public async Task<bool> UpdateAsync(
        Category category,
        string userId)
    {
        Category? existingCategory =
            await _repository
                .GetTrackedByIdForUserAsync(
                    category.Id,
                    userId);

        if (existingCategory == null)
        {
            return false;
        }


        existingCategory.Name =
            category.Name;


        await _repository
            .SaveChangesAsync();

        return true;
    }


    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        Category? category =
            await _repository
                .GetTrackedByIdForUserAsync(
                    id,
                    userId);

        if (category == null)
        {
            return false;
        }


        _repository.Remove(category);

        await _repository
            .SaveChangesAsync();

        return true;
    }


    public Task<bool> HasTasksAsync(
        int categoryId,
        string userId)
    {
        return _repository
            .HasTasksForUserAsync(
                categoryId,
                userId);
    }
}