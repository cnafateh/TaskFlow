using Microsoft.AspNetCore.Mvc;
using TaskFlow.Web.Models;
using TaskFlow.Web.Services;
using TaskFlow.Web.ViewModels.Projects;

namespace TaskFlow.Web.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        public async Task<IActionResult> Index()
        {
            List<Project> projects =
                await _projectService.GetAllAsync();

            return View(projects);
        }

        public async Task<IActionResult> Details(int id)
        {
            Project? project =
                await _projectService.GetByIdAsync(id);

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
        public async Task<IActionResult> Create(CreateProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Project project = new Project
            {
                Name = model.Name,
                Description = model.Description,
            };

            await _projectService.CreateAsync(project);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Project? project =
                await _projectService.GetByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            EditProjectViewModel model = new EditProjectViewModel
            {
                Id = id,
                Name = project.Name,
                Description = project.Description,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Project project = new Project
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
            };

            bool updated =
                await _projectService.UpdateAsync(project);

            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            Project? project =
                await _projectService.GetByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            DeleteProjectViewModel model = new DeleteProjectViewModel
            {
                Id = id,
                Name = project.Name,
                Description = project.Description,
            };

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
        DeleteProjectViewModel model)
        {
            bool deleted =
                await _projectService.DeleteAsync(model.Id);

            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
