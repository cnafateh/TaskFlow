using Microsoft.AspNetCore.Identity;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public ICollection<Project> Projects { get; set; }
        = new List<Project>();

    public ICollection<Category> Categories { get; set; }
        = new List<Category>();
}