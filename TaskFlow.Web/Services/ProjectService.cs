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

        public async Task<List<Project>> GetAllAsync()
        {
            return await _context.Project.ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(int id) 
        {
            return await _context.Project.FindAsync(id);
        }

        public async Task CreateAsync(Project project)
        {
            _context.Project.Add(project);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Project project)
        {
            Project? existingProject =
                await _context.Project.FindAsync(project.Id);

            if (existingProject == null)
            {
                return false;
            }

            existingProject.Name = project.Name;
            existingProject.Description = project.Description;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Project? project = await _context.Project.FindAsync(id);
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
