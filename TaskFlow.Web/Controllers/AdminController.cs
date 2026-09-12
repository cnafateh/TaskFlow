using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Web.Filters;
using TaskFlow.Web.Models;
using TaskFlow.Web.Services;
using TaskFlow.Web.ViewModels.Admin;

namespace TaskFlow.Web.Controllers;

[Authorize(Roles = "Admin")]
[ServiceFilter(typeof(ActionLoggingFilter))]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }


    public async Task<IActionResult> Index()
    {
        AdminDashboardViewModel model =
            await _adminService.GetDashboardAsync();

        return View(model);
    }


    public async Task<IActionResult> Users()
    {
        List<AdminUserViewModel> users =
            await _adminService.GetUsersAsync();

        return View(users);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(
        string userId,
        string role)
    {
        string? currentAdminId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (currentAdminId == null)
        {
            return Unauthorized();
        }


        string? error =
            await _adminService.UpdateUserRoleAsync(
                userId,
                role,
                currentAdminId);


        if (error != null)
        {
            TempData["ErrorMessage"] = error;

            return RedirectToAction(
                nameof(Users));
        }


        TempData["SuccessMessage"] =
            "User role updated successfully.";

        return RedirectToAction(
            nameof(Users));
    }

    public async Task<IActionResult> Tasks(
    AdminTaskIndexViewModel model)
    {
        AdminTaskFilter filter =
            new AdminTaskFilter
            {
                Search = model.Search,
                OwnerId = model.OwnerId,
                ProjectId = model.ProjectId,
                CategoryId = model.CategoryId,
                Priority = model.Priority,
                Status = model.Status,
                SortBy = model.SortBy,
                Page = model.Page
            };


        PagedResult<TaskItem> result =
            await _adminService.GetTasksAsync(
                filter);


        model.Tasks = result.Items;

        model.Page = result.Page;

        model.TotalPages = result.TotalPages;

        model.TotalCount = result.TotalCount;


        model.Owners =
            await _adminService
                .GetTaskOwnersAsync();

        model.Projects =
            await _adminService
                .GetAllProjectsAsync();

        model.Categories =
            await _adminService
                .GetAllCategoriesAsync();


        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTask(
    int id)
    {
        bool deleted =
            await _adminService
                .DeleteTaskAsync(id);


        if (!deleted)
        {
            TempData["ErrorMessage"] =
                "Task not found.";

            return RedirectToAction(
                nameof(Tasks));
        }


        TempData["SuccessMessage"] =
            "Task deleted successfully.";

        return RedirectToAction(
            nameof(Tasks));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkDeleteTasks(
    List<int> selectedTaskIds)
    {
        if (selectedTaskIds.Count == 0)
        {
            TempData["ErrorMessage"] =
                "No tasks were selected.";

            return RedirectToAction(
                nameof(Tasks));
        }


        int deletedCount =
            await _adminService
                .DeleteTasksAsync(
                    selectedTaskIds);


        if (deletedCount == 0)
        {
            TempData["ErrorMessage"] =
                "No tasks were deleted.";

            return RedirectToAction(
                nameof(Tasks));
        }


        TempData["SuccessMessage"] =
            $"{deletedCount} task(s) deleted successfully.";

        return RedirectToAction(
            nameof(Tasks));
    }
}