using Microsoft.AspNetCore.Mvc;
using TaskFlow.Web.Models;
using TaskFlow.Web.Services;
using TaskFlow.Web.ViewModels.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace TaskFlow.Web.Controllers;

public class TasksController : Controller
{
    private readonly ITaskService _taskService;
    private readonly IProjectService _projectService;

    public TasksController(ITaskService taskService, IProjectService projectService)
    {
        _taskService = taskService;
        _projectService = projectService;
    }

    public async Task<IActionResult> Index()
    {
        List<TaskItem> tasks =
            await _taskService.GetAllAsync();

        return View(tasks);
    }

    public async Task<IActionResult> Details(int id)
    {
        TaskItem? task =
            await _taskService.GetByIdAsync(id);

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
            Projects = await GetProjectOptionsAsync()
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
            return View(model);
        }

        TaskItem task = new TaskItem
        {
            Title = model.Title,
            Description = model.Description,
            DueDate = model.DueDate,
            IsCompleted = false,
            ProjectId = model.ProjectId,
        };

        await _taskService.CreateAsync(task);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        TaskItem? task =
            await _taskService.GetByIdAsync(id);

        if (task == null)
        {
            return NotFound();
        }

        EditTaskViewModel model = new EditTaskViewModel
        {
            Id = id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            ProjectId = task.ProjectId,
            Projects = await GetProjectOptionsAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditTaskViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Projects = await GetProjectOptionsAsync();
            return View(model);
        }

        TaskItem task = new TaskItem
        {
            Id= model.Id,
            Title = model.Title,
            Description = model.Description,
            DueDate = model.DueDate,
            IsCompleted = model.IsCompleted,
            ProjectId = model.ProjectId,
        };

        bool updated =
            await _taskService.UpdateAsync(task);

        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        TaskItem? task =
            await _taskService.GetByIdAsync(id);

        if (task == null)
        {
            return NotFound();
        }

        DeleteTaskViewModel model = new DeleteTaskViewModel 
        { 
            Id = id,
            Title = task.Title,
            Description= task.Description,
        };

        return View(model);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
    DeleteTaskViewModel model)
    {
        bool deleted =
            await _taskService.DeleteAsync(model.Id);

        if (!deleted)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetProjectOptionsAsync()
    {
        List<Project> projects =
            await _projectService.GetAllAsync();

        return projects
            .Select(project => new SelectListItem
            {
                Value = project.Id.ToString(),
                Text = project.Name
            })
            .ToList();
    }
}