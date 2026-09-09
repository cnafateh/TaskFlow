using Microsoft.EntityFrameworkCore;
using TaskFlow.Web.Data;
using TaskFlow.Web.Models;

namespace TaskFlow.Web.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;

        public ProjectService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetAllAsync(string userId)
        {
            return await _context.Project
                .Where(project => project.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(
            int id,
            string userId)
        {
            return await _context.Project
                .AsNoTracking()
                .FirstOrDefaultAsync(project =>
                    project.Id == id &&
                    project.UserId == userId);
        }

        public async Task CreateAsync(Project project)
        {
            _context.Project.Add(project);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(
            Project project,
            string userId)
        {
            Project? existingProject =
                await _context.Project
                    .FirstOrDefaultAsync(p =>
                        p.Id == project.Id &&
                        p.UserId == userId);

            if (existingProject == null)
            {
                return false;
            }

            existingProject.Name = project.Name;
            existingProject.Description = project.Description;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(
            int id,
            string userId)
        {
            Project? project =
                await _context.Project
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.UserId == userId);

            if (project == null)
            {
                return false;
            }

            _context.Project.Remove(project);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
