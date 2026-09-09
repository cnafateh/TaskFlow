using TaskFlow.Web.Models;

namespace TaskFlow.Web.Services
{
    public interface IProjectService
    {
        Task<List<Project>> GetAllAsync(string userId);

        Task<Project?> GetByIdAsync(int id, string userId);

        Task CreateAsync(Project project);

        Task<bool> UpdateAsync(Project project, string userId);

        Task<bool> DeleteAsync(int id, string userId);
    }
}
