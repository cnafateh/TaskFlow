using Microsoft.AspNetCore.Mvc;
using TaskFlow.Web.Models;


namespace TaskFlow.Web.Controllers;

public class TasksController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Details(int id)
    {
        ViewBag.TaskId = id;

        return View();
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(TaskItem task)
    {
        Console.WriteLine(task.Title);
        Console.WriteLine(task.Description);
        Console.WriteLine(task.IsCompleted);
        return View();
    }
}