using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskFlow.Web.Models;
using TaskFlow.Web.Services;
using TaskFlow.Web.ViewModels.Tasks;


namespace TaskFlow.Web.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly ITaskService _taskService;
    private readonly IProjectService _projectService;
    private readonly ICategoryService _categoryService;
    private readonly UserManager<ApplicationUser> _userManager;

    private string? GetCurrentUserId()
    {
        return _userManager.GetUserId(User);
    }
    public TasksController(
    ITaskService taskService,
    IProjectService projectService,
    ICategoryService categoryService,
    UserManager<ApplicationUser> userManager)
    {
        _taskService = taskService;
        _projectService = projectService;
        _categoryService = categoryService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(TaskIndexViewModel model)
    {
        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        TaskFilter filter = new TaskFilter
        {
            Search = model.Search,
            ProjectId = model.ProjectId,
            CategoryId = model.CategoryId,
            Priority = model.Priority,
            Status = model.Status,
            SortBy = model.SortBy,
            Page = model.Page,
        };

        PagedResult<TaskItem> result =
            await _taskService.GetFilteredAsync(filter, userId);

        model.Tasks = result.Items;
        model.TotalPages = result.TotalPages;
        model.TotalCount = result.TotalCount;
        model.Page = result.Page;

        model.Projects =
            await GetProjectOptionsAsync();

        model.Categories =
            await GetCategoryOptionsAsync();

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        TaskItem? task =
            await _taskService.GetByIdAsync(id, userId);

        if (task == null)
        {
            return NotFound();
        }

        return View(task);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        CreateTaskViewModel model = new CreateTaskViewModel
        {
            Projects = await GetProjectOptionsAsync(),
            Categories = await GetCategoryOptionsAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTaskViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Projects = await GetProjectOptionsAsync();
            model.Categories = await GetCategoryOptionsAsync();

            return View(model);
        }

        TaskItem task = new TaskItem
        {
            Title = model.Title,
            Description = model.Description,
            DueDate = model.DueDate,
            ProjectId = model.ProjectId,
            CategoryId = model.CategoryId,
            Priority = model.Priority,
            Status = Models.Enums.TaskStatus.Todo
        };

        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        bool created =
            await _taskService.CreateAsync(task, userId);

        if (!created)
        {
            return BadRequest();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        TaskItem? task =
            await _taskService.GetByIdAsync(id, userId);

        if (task == null)
        {
            return NotFound();
        }

        EditTaskViewModel model = new EditTaskViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            ProjectId = task.ProjectId,
            Projects = await GetProjectOptionsAsync(),
            CategoryId = task.CategoryId,
            Categories = await GetCategoryOptionsAsync(),
            Priority = task.Priority,
            Status = task.Status
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditTaskViewModel model)
    {
        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            model.Projects = await GetProjectOptionsAsync();
            model.Categories = await GetCategoryOptionsAsync();

            return View(model);
        }

        TaskItem task = new TaskItem
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            DueDate = model.DueDate,
            ProjectId = model.ProjectId,
            CategoryId = model.CategoryId,
            Priority = model.Priority,
            Status = model.Status
        };

        bool updated =
            await _taskService.UpdateAsync(task, userId);

        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        TaskItem? task =
            await _taskService.GetByIdAsync(id, userId);

        if (task == null)
        {
            return NotFound();
        }

        DeleteTaskViewModel model = new DeleteTaskViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description
        };

        return View(model);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
    DeleteTaskViewModel model)
    {
        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        bool deleted =
            await _taskService.DeleteAsync(model.Id, userId);

        if (!deleted)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetProjectOptionsAsync()
    {
        string? userId = _userManager.GetUserId(User);

        if (userId == null)
        {
            return new List<SelectListItem>();
        }

        List<Project> projects =
            await _projectService.GetAllAsync(userId);

        return projects
            .Select(project => new SelectListItem
            {
                Value = project.Id.ToString(),
                Text = project.Name
            })
            .ToList();
    }

    private async Task<List<SelectListItem>>
    GetCategoryOptionsAsync()
    {
        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return new List<SelectListItem>();
        }

        List<Category> categories =
            await _categoryService.GetAllAsync(userId);

        return categories
            .Select(category => new SelectListItem
            {
                Value = category.Id.ToString(),
                Text = category.Name
            })
            .ToList();
    }
}