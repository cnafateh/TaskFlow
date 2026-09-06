using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Web.Data;
using TaskFlow.Web.Models;
using TaskFlow.Web.Services;


namespace TaskFlow.Web.Controllers;

public class TasksController : Controller{

    private readonly AppDbContext _context;
    private readonly ITaskService _taskService;

    public TasksController(
    AppDbContext context,
    ITaskService taskService)
    {
        _context = context;
        _taskService = taskService;
    }


    public async Task<IActionResult> Index()
    {
        List<TaskItem> tasks = await _taskService.GetAllAsync();

        return View(tasks);
    }

    public async Task<IActionResult> Details(int id)
    {
        TaskItem? task = await _taskService.GetByIdAsync(id);

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
        TaskItem? task = await _context.Tasks.FindAsync(id);

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

        TaskItem? existingTask =
            await _context.Tasks.FindAsync(task.Id);

        if (existingTask == null)
        {
            return NotFound();
        }

        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.IsCompleted = task.IsCompleted;
        existingTask.DueDate = task.DueDate;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        TaskItem? task =
            await _context.Tasks.FindAsync(id);

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
        TaskItem? task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}