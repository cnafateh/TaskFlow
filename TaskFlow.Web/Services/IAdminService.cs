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
}