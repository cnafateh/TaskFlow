using TaskFlow.Application.Common;
using TaskFlow.Application.Categories;
using TaskFlow.Application.Projects;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Tasks;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICategoryRepository _categoryRepository;


    public TaskService(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        ICategoryRepository categoryRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _categoryRepository = categoryRepository;
    }


    public Task<TaskItem?> GetByIdAsync(
        int id,
        string userId)
    {
        return _taskRepository
            .GetByIdForUserAsync(
                id,
                userId);
    }


    public async Task<bool> CreateAsync(
        TaskItem task,
        string userId)
    {
        bool projectExists =
            await _projectRepository
                .ExistsForUserAsync(
                    task.ProjectId,
                    userId);

        if (!projectExists)
        {
            return false;
        }


        bool categoryExists =
            await _categoryRepository
                .ExistsForUserAsync(
                    task.CategoryId,
                    userId);

        if (!categoryExists)
        {
            return false;
        }


        await _taskRepository
            .AddAsync(task);

        await _taskRepository
            .SaveChangesAsync();

        return true;
    }


    public async Task<bool> UpdateAsync(
        TaskItem task,
        string userId)
    {
        TaskItem? existingTask =
            await _taskRepository
                .GetTrackedByIdForUserAsync(
                    task.Id,
                    userId);

        if (existingTask == null)
        {
            return false;
        }


        bool projectExists =
            await _projectRepository
                .ExistsForUserAsync(
                    task.ProjectId,
                    userId);

        if (!projectExists)
        {
            return false;
        }


        bool categoryExists =
            await _categoryRepository
                .ExistsForUserAsync(
                    task.CategoryId,
                    userId);

        if (!categoryExists)
        {
            return false;
        }


        existingTask.Title =
            task.Title;

        existingTask.Description =
            task.Description;

        existingTask.DueDate =
            task.DueDate;

        existingTask.ProjectId =
            task.ProjectId;

        existingTask.CategoryId =
            task.CategoryId;

        existingTask.Priority =
            task.Priority;

        existingTask.Status =
            task.Status;

        existingTask.UpdatedAt =
            DateTime.UtcNow;


        await _taskRepository
            .SaveChangesAsync();

        return true;
    }


    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        TaskItem? task =
            await _taskRepository
                .GetTrackedByIdForUserAsync(
                    id,
                    userId);

        if (task == null)
        {
            return false;
        }


        _taskRepository.Remove(task);

        await _taskRepository
            .SaveChangesAsync();

        return true;
    }


    public Task<PagedResult<TaskItem>> GetFilteredAsync(
        TaskFilter filter,
        string userId)
    {
        return _taskRepository
            .GetFilteredAsync(
                filter,
                userId);
    }


    public Task<TaskSummary> GetSummaryAsync(
        string userId)
    {
        return _taskRepository
            .GetSummaryAsync(userId);
    }
}