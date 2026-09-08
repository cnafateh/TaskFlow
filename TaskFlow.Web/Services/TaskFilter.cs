using TaskFlow.Web.Models.Enums;

namespace TaskFlow.Web.Services;

public class TaskFilter
{
    public string? Search { get; set; }

    public int? ProjectId { get; set; }

    public int? CategoryId { get; set; }

    public TaskPriority? Priority { get; set; }

    public Models.Enums.TaskStatus? Status { get; set; }

    public string? SortBy { get; set; }
}