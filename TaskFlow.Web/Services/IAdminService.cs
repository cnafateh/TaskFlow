using TaskFlow.Web.Models;
using TaskFlow.Web.ViewModels.Admin;

namespace TaskFlow.Web.Services;

public interface IAdminService
{
    Task<AdminDashboardViewModel> GetDashboardAsync();

    Task<List<AdminUserViewModel>> GetUsersAsync();

    Task<string?> UpdateUserRoleAsync(
        string userId,
        string role,
        string currentAdminId);

    Task<PagedResult<TaskItem>> GetTasksAsync(
    AdminTaskFilter filter);

    Task<List<ApplicationUser>> GetTaskOwnersAsync();

    Task<List<Project>> GetAllProjectsAsync();

    Task<List<Category>> GetAllCategoriesAsync();

    Task<bool> DeleteTaskAsync(int id);

    Task<int> DeleteTasksAsync(IEnumerable<int> ids);
}