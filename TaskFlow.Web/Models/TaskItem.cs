using System.ComponentModel.DataAnnotations;
using TaskFlow.Web.Models.Enums;

namespace TaskFlow.Web.Models;

public class TaskItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Task title is required.")]
    [StringLength(
    100,
    ErrorMessage = "Title cannot be longer than 100 characters.")]
    public string Title { get; set; } = "";

    [StringLength(
        500,
        ErrorMessage = "Description cannot be longer than 500 characters.")]
    public string Description { get; set; } = "";

    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; }

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;


    public TaskPriority Priority { get; set; }
    = TaskPriority.Medium;

    public Enums.TaskStatus Status { get; set; }
        = Enums.TaskStatus.Todo;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}