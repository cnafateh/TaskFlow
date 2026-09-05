using Microsoft.AspNetCore.Mvc;

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

    public IActionResult Create()
    {
        return View();
    }
}