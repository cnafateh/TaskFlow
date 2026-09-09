using Microsoft.EntityFrameworkCore;
using TaskFlow.Web.Data;
using TaskFlow.Web.Models;

namespace TaskFlow.Web.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync(
        string userId)
    {
        return await _context.Categories
            .Where(category =>
                category.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(
        int id,
        string userId)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category =>
                category.Id == id &&
                category.UserId == userId);
    }

    public async Task CreateAsync(Category category)
    {
        _context.Categories.Add(category);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(
        Category category,
        string userId)
    {
        Category? existingCategory =
            await _context.Categories
                .FirstOrDefaultAsync(existing =>
                    existing.Id == category.Id &&
                    existing.UserId == userId);

        if (existingCategory == null)
        {
            return false;
        }

        existingCategory.Name = category.Name;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        Category? category =
            await _context.Categories
                .FirstOrDefaultAsync(category =>
                    category.Id == id &&
                    category.UserId == userId);

        if (category == null)
        {
            return false;
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> HasTasksAsync(
        int categoryId,
        string userId)
    {
        return await _context.Tasks
            .AnyAsync(task =>
                task.CategoryId == categoryId &&
                task.Category.UserId == userId);
    }
}