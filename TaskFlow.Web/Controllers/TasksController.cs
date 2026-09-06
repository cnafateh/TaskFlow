using Microsoft.AspNetCore.Mvc;
using TaskFlow.Web.Models;
using TaskFlow.Web.Services;

namespace TaskFlow.Web.Controllers;

public class TasksController : Controller
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
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
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskItem task)
    {
        if (!ModelState.IsValid)
        {
            return View(task);
        }

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

        return View(task);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TaskItem task)
    {
        if (!ModelState.IsValid)
        {
            return View(task);
        }

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

        return View(task);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        bool deleted =
            await _taskService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}