namespace TaskFlow.Web.ViewModels.Admin;

public class AdminProjectRowViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public string OwnerEmail { get; set; } = "";

    public int TaskCount { get; set; }

    public DateTime CreatedAt { get; set; }
}