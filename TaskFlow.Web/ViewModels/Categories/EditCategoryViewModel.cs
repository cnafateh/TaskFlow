using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Web.ViewModels.Categories
{
    public class EditCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(
        100,
        ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; } = "";
    }
}
