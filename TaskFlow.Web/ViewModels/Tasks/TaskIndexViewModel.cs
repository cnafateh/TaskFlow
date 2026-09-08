using Microsoft.AspNetCore.Mvc.Rendering;
using TaskFlow.Web.Models;
using TaskFlow.Web.Models.Enums;

namespace TaskFlow.Web.ViewModels.Tasks;

public class TaskIndexViewModel
{
    public List<TaskItem> Tasks { get; set; }
        = new List<TaskItem>();

    public string? Search { get; set; }

    public int? ProjectId { get; set; }

    public int? CategoryId { get; set; }

    public TaskPriority? Priority { get; set; }

    public Models.Enums.TaskStatus? Status { get; set; }

    public string? SortBy { get; set; }

    public List<SelectListItem> Projects { get; set; }
        = new List<SelectListItem>();

    public List<SelectListItem> Categories { get; set; }
        = new List<SelectListItem>();
}