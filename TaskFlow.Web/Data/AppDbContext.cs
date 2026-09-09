using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Web.Models;

namespace TaskFlow.Web.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; }

    public DbSet<Project> Project { get; set; }

    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Project -> Tasks
        modelBuilder.Entity<TaskItem>()
            .HasOne(task => task.Project)
            .WithMany(project => project.Tasks)
            .HasForeignKey(task => task.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category -> Tasks
        modelBuilder.Entity<TaskItem>()
            .HasOne(task => task.Category)
            .WithMany(category => category.Tasks)
            .HasForeignKey(task => task.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // User -> Projects
        modelBuilder.Entity<Project>()
            .HasOne(project => project.User)
            .WithMany(user => user.Projects)
            .HasForeignKey(project => project.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> Categories
        modelBuilder.Entity<Category>()
            .HasOne(category => category.User)
            .WithMany(user => user.Categories)
            .HasForeignKey(category => category.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}