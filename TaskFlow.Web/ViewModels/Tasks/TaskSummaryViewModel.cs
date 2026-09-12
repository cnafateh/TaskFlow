namespace TaskFlow.Web.ViewModels.Tasks;

public class TaskSummaryViewModel
{
    public int TotalTasks { get; set; }

    public int InProgressTasks { get; set; }

    public int CompletedTasks { get; set; }
}