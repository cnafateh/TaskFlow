namespace TaskFlow.Web.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }

    public int TotalAdmins { get; set; }

    public int TotalProjects { get; set; }

    public int TotalTasks { get; set; }

    public int CompletedTasks { get; set; }
}