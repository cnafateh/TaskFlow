using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Projects;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<List<Project>> GetAllForUserAsync(
        string userId)
    {
        return await _context.Project
            .Where(project =>
                project.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }


    public async Task<Project?> GetByIdForUserAsync(
        int id,
        string userId)
    {
        return await _context.Project
            .AsNoTracking()
            .FirstOrDefaultAsync(project =>
                project.Id == id &&
                project.UserId == userId);
    }


    public async Task<Project?> GetTrackedByIdForUserAsync(
        int id,
        string userId)
    {
        return await _context.Project
            .FirstOrDefaultAsync(project =>
                project.Id == id &&
                project.UserId == userId);
    }


    public async Task<bool> ExistsForUserAsync(
        int id,
        string userId)
    {
        return await _context.Project
            .AnyAsync(project =>
                project.Id == id &&
                project.UserId == userId);
    }


    public async Task AddAsync(Project project)
    {
        await _context.Project.AddAsync(project);
    }


    public void Remove(Project project)
    {
        _context.Project.Remove(project);
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}