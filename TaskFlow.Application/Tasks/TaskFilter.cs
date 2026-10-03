using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Tasks;

public class TaskFilter
{
    public string? Search { get; set; }

    public int? ProjectId { get; set; }

    public int? CategoryId { get; set; }

    public TaskPriority? Priority { get; set; }

    public TaskFlow.Domain.Enums.TaskStatus? Status { get; set; }

    public string? SortBy { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}