using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Web.Models;

public class Project
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Project name is required.")]
    [StringLength(
        100,
        ErrorMessage = "Name cannot be longer than 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(
        500,
        ErrorMessage = "Description cannot be longer than 500 characters.")]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TaskItem> Tasks { get; set; }
        = new List<TaskItem>();

    public string? UserId { get; set; }

    public ApplicationUser? User { get; set; }
}