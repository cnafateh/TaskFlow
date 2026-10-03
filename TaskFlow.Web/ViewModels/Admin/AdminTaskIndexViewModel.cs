using TaskFlow.Web.Models;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Web.ViewModels.Admin;

public class AdminTaskIndexViewModel
{
    public List<TaskItem> Tasks { get; set; } = new();


    public string? Search { get; set; }

    public string? OwnerId { get; set; }

    public int? ProjectId { get; set; }

    public int? CategoryId { get; set; }

    public TaskPriority? Priority { get; set; }

    public TaskFlow.Domain.Enums.TaskStatus? Status { get; set; }

    public string? SortBy { get; set; }


    public List<ApplicationUser> Owners { get; set; } = new();

    public List<Project> Projects { get; set; } = new();

    public List<Category> Categories { get; set; } = new();


    public int Page { get; set; } = 1;

    public int TotalPages { get; set; }

    public int TotalCount { get; set; }
}