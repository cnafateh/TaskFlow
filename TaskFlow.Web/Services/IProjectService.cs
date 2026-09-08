using TaskFlow.Web.Models;

namespace TaskFlow.Web.Services
{
    public interface IProjectService
    {
        Task<List<Project>> GetAllAsync();

        Task<Project?> GetByIdAsync(int id);

        Task CreateAsync(Project project);

        Task<bool> UpdateAsync(Project project);

        Task<bool> DeleteAsync(int id);
    }
}
