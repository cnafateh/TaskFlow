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


    Task<PagedResult<AdminProjectRowViewModel>>
    GetProjectsAsync(AdminProjectFilter filter);

    Task<bool> DeleteProjectAsync(int id);

    Task<int> DeleteProjectsAsync(
        IEnumerable<int> ids);


    Task<PagedResult<AdminCategoryRowViewModel>>
        GetCategoriesAsync(AdminCategoryFilter filter);

    Task<bool> DeleteCategoryAsync(int id);

    Task<AdminBulkDeleteResult> DeleteCategoriesAsync(
        IEnumerable<int> ids);
}