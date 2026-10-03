using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Categories;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<List<Category>> GetAllForUserAsync(
        string userId)
    {
        return await _context.Categories
            .Where(category =>
                category.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }


    public async Task<Category?> GetByIdForUserAsync(
        int id,
        string userId)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category =>
                category.Id == id &&
                category.UserId == userId);
    }


    public async Task<Category?> GetTrackedByIdForUserAsync(
        int id,
        string userId)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(category =>
                category.Id == id &&
                category.UserId == userId);
    }


    public async Task<bool> ExistsForUserAsync(
        int id,
        string userId)
    {
        return await _context.Categories
            .AnyAsync(category =>
                category.Id == id &&
                category.UserId == userId);
    }


    public async Task<bool> HasTasksForUserAsync(
        int categoryId,
        string userId)
    {
        return await _context.Tasks
            .AnyAsync(task =>
                task.CategoryId == categoryId &&
                task.Category.UserId == userId);
    }


    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
    }


    public void Remove(Category category)
    {
        _context.Categories.Remove(category);
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}