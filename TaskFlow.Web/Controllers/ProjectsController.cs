using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Web.Models;
using TaskFlow.Web.Services;
using TaskFlow.Web.ViewModels.Projects;

namespace TaskFlow.Web.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly IProjectService _projectService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectsController(
        IProjectService projectService,
        UserManager<ApplicationUser> userManager)
    {
        _projectService = projectService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        List<Project> projects =
            await _projectService.GetAllAsync(userId);

        return View(projects);
    }

    public async Task<IActionResult> Details(int id)
    {
        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        Project? project =
            await _projectService.GetByIdAsync(id, userId);

        if (project == null)
        {
            return NotFound();
        }

        return View(project);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateProjectViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        Project project = new Project
        {
            Name = model.Name,
            Description = model.Description,
            UserId = userId
        };

        await _projectService.CreateAsync(project);

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

        Project? project =
            await _projectService.GetByIdAsync(id, userId);

        if (project == null)
        {
            return NotFound();
        }

        EditProjectViewModel model =
            new EditProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description
            };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EditProjectViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        Project project = new Project
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description
        };

        bool updated =
            await _projectService.UpdateAsync(
                project,
                userId);

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

        Project? project =
            await _projectService.GetByIdAsync(
                id,
                userId);

        if (project == null)
        {
            return NotFound();
        }

        DeleteProjectViewModel model =
            new DeleteProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description
            };

        return View(model);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        DeleteProjectViewModel model)
    {
        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        bool deleted =
            await _projectService.DeleteAsync(
                model.Id,
                userId);

        if (!deleted)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private string? GetCurrentUserId()
    {
        return _userManager.GetUserId(User);
    }
}