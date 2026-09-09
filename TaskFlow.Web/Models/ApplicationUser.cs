using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Web.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<Project> Projects { get; set; }
        = new List<Project>();

    public ICollection<Category> Categories { get; set; }
        = new List<Category>();
}