using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Projects;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;


    public ProjectService(
        IProjectRepository repository)
    {
        _repository = repository;
    }


    public Task<List<Project>> GetAllAsync(
        string userId)
    {
        return _repository
            .GetAllForUserAsync(userId);
    }


    public Task<Project?> GetByIdAsync(
        int id,
        string userId)
    {
        return _repository
            .GetByIdForUserAsync(
                id,
                userId);
    }


    public async Task CreateAsync(
        Project project)
    {
        await _repository.AddAsync(project);
        await _repository.SaveChangesAsync();
    }


    public async Task<bool> UpdateAsync(
        Project project,
        string userId)
    {
        Project? existingProject =
            await _repository
                .GetTrackedByIdForUserAsync(
                    project.Id,
                    userId);

        if (existingProject == null)
        {
            return false;
        }


        existingProject.Name =
            project.Name;

        existingProject.Description =
            project.Description;


        await _repository
            .SaveChangesAsync();

        return true;
    }


    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        Project? project =
            await _repository
                .GetTrackedByIdForUserAsync(
                    id,
                    userId);

        if (project == null)
        {
            return false;
        }


        _repository.Remove(project);

        await _repository
            .SaveChangesAsync();

        return true;
    }
}