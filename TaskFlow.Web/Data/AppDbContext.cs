using Microsoft.EntityFrameworkCore;
using TaskFlow.Web.Models;

namespace TaskFlow.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Project> Project {  get; set; }
}