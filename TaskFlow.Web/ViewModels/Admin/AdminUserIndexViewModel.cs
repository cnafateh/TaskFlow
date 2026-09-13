namespace TaskFlow.Web.ViewModels.Admin;

public class AdminUserIndexViewModel
{
    public List<AdminUserViewModel> Users { get; set; } = [];

    public string? Search { get; set; }

    public string? Role { get; set; }

    public string? SortBy { get; set; }

    public int Page { get; set; }

    public int TotalPages { get; set; }

    public int TotalCount { get; set; }
}