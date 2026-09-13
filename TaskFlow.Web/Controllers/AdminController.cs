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


    [HttpGet]
    public async Task<IActionResult> Users(
    string? search,
    string? role,
    string? sortBy,
    int page = 1)
    {
        AdminUserFilter filter =
            new()
            {
                Search = search,
                Role = role,
                SortBy = sortBy,
                Page = page,
                PageSize = 10
            };


        PagedResult<AdminUserViewModel> result =
            await _adminService.GetUsersAsync(filter);


        AdminUserIndexViewModel model =
            new()
            {
                Users = result.Items,
                Search = search,
                Role = role,
                SortBy = sortBy,
                Page = result.Page,
                TotalPages = result.TotalPages,
                TotalCount = result.TotalCount
            };


        return View(model);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkTaskAction(
    string? bulkAction,
    List<int>? selectedTaskIds)
    {
        if (selectedTaskIds == null ||
            selectedTaskIds.Count == 0)
        {
            TempData["ErrorMessage"] =
                "Select at least one task.";

            return RedirectToAction(
                nameof(Tasks));
        }


        int affectedCount;


        switch (bulkAction)
        {
            case "todo":

                affectedCount =
                    await _adminService
                        .UpdateTasksStatusAsync(
                            selectedTaskIds,
                            Models.Enums.TaskStatus.Todo);

                TempData["SuccessMessage"] =
                    $"{affectedCount} task(s) marked as Todo.";

                break;


            case "inProgress":

                affectedCount =
                    await _adminService
                        .UpdateTasksStatusAsync(
                            selectedTaskIds,
                            Models.Enums.TaskStatus.InProgress);

                TempData["SuccessMessage"] =
                    $"{affectedCount} task(s) marked as In Progress.";

                break;


            case "done":

                affectedCount =
                    await _adminService
                        .UpdateTasksStatusAsync(
                            selectedTaskIds,
                            Models.Enums.TaskStatus.Done);

                TempData["SuccessMessage"] =
                    $"{affectedCount} task(s) marked as Done.";

                break;


            case "delete":

                affectedCount =
                    await _adminService
                        .DeleteTasksAsync(
                            selectedTaskIds);

                TempData["SuccessMessage"] =
                    $"{affectedCount} task(s) deleted.";

                break;


            default:

                TempData["ErrorMessage"] =
                    "Select a valid bulk action.";

                break;
        }


        return RedirectToAction(
            nameof(Tasks));
    }

    public async Task<IActionResult> Projects(
    AdminProjectIndexViewModel model)
    {
        AdminProjectFilter filter =
            new AdminProjectFilter
            {
                Search =
                    model.Search,

                OwnerId =
                    model.OwnerId,

                SortBy =
                    model.SortBy,

                Page =
                    model.Page
            };


        PagedResult<AdminProjectRowViewModel>
            result =
                await _adminService
                    .GetProjectsAsync(filter);


        model.Projects =
            result.Items;

        model.Page =
            result.Page;

        model.TotalPages =
            result.TotalPages;

        model.TotalCount =
            result.TotalCount;

        model.Owners =
            await _adminService
                .GetTaskOwnersAsync();


        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult>
    DeleteProject(int id)
    {
        bool deleted =
            await _adminService
                .DeleteProjectAsync(id);


        if (!deleted)
        {
            TempData["ErrorMessage"] =
                "Project not found.";
        }
        else
        {
            TempData["SuccessMessage"] =
                "Project and its tasks were deleted successfully.";
        }


        return RedirectToAction(
            nameof(Projects));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult>
    BulkDeleteProjects(
        List<int> selectedProjectIds)
    {
        if (selectedProjectIds.Count == 0)
        {
            TempData["ErrorMessage"] =
                "No projects were selected.";

            return RedirectToAction(
                nameof(Projects));
        }


        int deletedCount =
            await _adminService
                .DeleteProjectsAsync(
                    selectedProjectIds);


        TempData["SuccessMessage"] =
            $"{deletedCount} project(s) deleted successfully.";


        return RedirectToAction(
            nameof(Projects));
    }

    public async Task<IActionResult> Categories(
    AdminCategoryIndexViewModel model)
    {
        AdminCategoryFilter filter =
            new AdminCategoryFilter
            {
                Search =
                    model.Search,

                OwnerId =
                    model.OwnerId,

                SortBy =
                    model.SortBy,

                Page =
                    model.Page
            };


        PagedResult<AdminCategoryRowViewModel>
            result =
                await _adminService
                    .GetCategoriesAsync(filter);


        model.Categories =
            result.Items;

        model.Page =
            result.Page;

        model.TotalPages =
            result.TotalPages;

        model.TotalCount =
            result.TotalCount;

        model.Owners =
            await _adminService
                .GetTaskOwnersAsync();


        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult>
    DeleteCategory(int id)
    {
        bool deleted =
            await _adminService
                .DeleteCategoryAsync(id);


        if (!deleted)
        {
            TempData["ErrorMessage"] =
                "Category could not be deleted. It may still be assigned to tasks.";
        }
        else
        {
            TempData["SuccessMessage"] =
                "Category deleted successfully.";
        }


        return RedirectToAction(
            nameof(Categories));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult>
    BulkDeleteCategories(
        List<int> selectedCategoryIds)
    {
        if (selectedCategoryIds.Count == 0)
        {
            TempData["ErrorMessage"] =
                "No categories were selected.";

            return RedirectToAction(
                nameof(Categories));
        }


        AdminBulkDeleteResult result =
            await _adminService
                .DeleteCategoriesAsync(
                    selectedCategoryIds);


        if (result.DeletedCount > 0)
        {
            TempData["SuccessMessage"] =
                $"{result.DeletedCount} category(s) deleted successfully.";
        }


        if (result.SkippedCount > 0)
        {
            TempData["ErrorMessage"] =
                $"{result.SkippedCount} category(s) were skipped because they are assigned to tasks.";
        }


        return RedirectToAction(
            nameof(Categories));
    }
}