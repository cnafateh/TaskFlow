using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Web.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = "";

    public ICollection<TaskItem> Tasks { get; set; }
        = new List<TaskItem>();
}