using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Web.Models;
using TaskFlow.Web.Services;
using TaskFlow.Web.ViewModels.Tasks;

namespace TaskFlow.Web.ViewComponents;

public class TaskSummaryViewComponent : ViewComponent
{
    private readonly ITaskService _taskService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TaskSummaryViewComponent(
        ITaskService taskService,
        UserManager<ApplicationUser> userManager)
    {
        _taskService = taskService;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        string? userId =
            _userManager.GetUserId(
                HttpContext.User);

        if (userId == null)
        {
            return Content(string.Empty);
        }

        TaskSummaryViewModel model =
            await _taskService.GetSummaryAsync(userId);

        return View(model);
    }
}