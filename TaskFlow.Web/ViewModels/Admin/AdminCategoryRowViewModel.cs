namespace TaskFlow.Web.ViewModels.Admin;

public class AdminCategoryRowViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string OwnerEmail { get; set; } = "";

    public int TaskCount { get; set; }
}