using TaskFlow.Web.Models;

namespace TaskFlow.Web.ViewModels.Admin;

public class AdminProjectIndexViewModel
{
    public List<AdminProjectRowViewModel> Projects { get; set; } = new();

    public List<ApplicationUser> Owners { get; set; } = new();

    public string? Search { get; set; }

    public string? OwnerId { get; set; }

    public string? SortBy { get; set; }

    public int Page { get; set; } = 1;

    public int TotalPages { get; set; }

    public int TotalCount { get; set; }
}