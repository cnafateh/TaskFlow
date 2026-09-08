using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TaskFlow.Web.Models.Enums;

namespace TaskFlow.Web.ViewModels.Tasks;

public class CreateTaskViewModel
{
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


    [Range(1, int.MaxValue, ErrorMessage = "Please select a project.")]
    public int ProjectId { get; set; }

    public List<SelectListItem> Projects { get; set; }
    = new List<SelectListItem>();

    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    public int CategoryId { get; set; }

    public List<SelectListItem> Categories { get; set; }
        = new List<SelectListItem>();

    public TaskPriority Priority { get; set; }
        = TaskPriority.Medium;
}