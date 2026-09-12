namespace TaskFlow.Web.Services;

public class AdminCategoryFilter
{
    public string? Search { get; set; }

    public string? OwnerId { get; set; }

    public string? SortBy { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}