using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Tests.Infrastructure;
using TaskFlow.Web.Models;

namespace TaskFlow.Tests.Services;

public class AdminServiceTests
{
    [Fact]
    public async Task UpdateUserRoleAsync_WhenAdminChangesOwnRole_ReturnsError()
    {
        await using AdminTestEnvironment environment =
            await AdminTestEnvironment.CreateAsync();


        ApplicationUser admin =
            await CreateUserAsync(
                environment.UserManager,
                "admin-1",
                "admin1@example.com",
                "Admin");


        string? result =
            await environment.AdminService.UpdateUserRoleAsync(
                admin.Id,
                "User",
                admin.Id);


        Assert.Equal(
            "You cannot change your own role.",
            result);

        Assert.True(
            await environment.UserManager
                .IsInRoleAsync(admin, "Admin"));
    }


    [Fact]
    public async Task UpdateUserRoleAsync_WhenLastAdminIsDowngraded_ReturnsError()
    {
        await using AdminTestEnvironment environment =
            await AdminTestEnvironment.CreateAsync();


        ApplicationUser admin =
            await CreateUserAsync(
                environment.UserManager,
                "admin-1",
                "admin1@example.com",
                "Admin");


        string? result =
            await environment.AdminService.UpdateUserRoleAsync(
                admin.Id,
                "User",
                "another-admin");


        Assert.Equal(
            "The last administrator cannot be removed.",
            result);

        Assert.True(
            await environment.UserManager
                .IsInRoleAsync(admin, "Admin"));
    }


    [Fact]
    public async Task UpdateUserRoleAsync_WhenUserIsPromoted_ChangesRole()
    {
        await using AdminTestEnvironment environment =
            await AdminTestEnvironment.CreateAsync();


        ApplicationUser admin =
            await CreateUserAsync(
                environment.UserManager,
                "admin-1",
                "admin1@example.com",
                "Admin");


        ApplicationUser user =
            await CreateUserAsync(
                environment.UserManager,
                "user-1",
                "user1@example.com",
                "User");


        string? result =
            await environment.AdminService.UpdateUserRoleAsync(
                user.Id,
                "Admin",
                admin.Id);


        Assert.Null(result);

        Assert.True(
            await environment.UserManager
                .IsInRoleAsync(user, "Admin"));

        Assert.False(
            await environment.UserManager
                .IsInRoleAsync(user, "User"));
    }


    [Fact]
    public async Task DeleteCategoryAsync_WhenCategoryHasTasks_ReturnsFalse()
    {
        await using AdminTestEnvironment environment =
            await AdminTestEnvironment.CreateAsync();


        ApplicationUser user =
            await CreateUserAsync(
                environment.UserManager,
                "user-1",
                "user1@example.com",
                "User");


        Project project = new()
        {
            Name = "Project",
            Description = "",
            UserId = user.Id
        };

        Category category = new()
        {
            Name = "Category",
            UserId = user.Id
        };


        environment.Context.Project.Add(project);
        environment.Context.Categories.Add(category);

        await environment.Context.SaveChangesAsync();


        TaskItem task = new()
        {
            Title = "Task",
            Description = "",
            DueDate = DateTime.UtcNow.AddDays(1),

            ProjectId = project.Id,
            CategoryId = category.Id
        };


        environment.Context.Tasks.Add(task);

        await environment.Context.SaveChangesAsync();


        bool result =
            await environment.AdminService
                .DeleteCategoryAsync(category.Id);


        Assert.False(result);

        Assert.True(
            await environment.Context.Categories
                .AnyAsync(existing =>
                    existing.Id == category.Id));
    }


    [Fact]
    public async Task DeleteTasksAsync_DeletesOnlySelectedTasks()
    {
        await using AdminTestEnvironment environment =
            await AdminTestEnvironment.CreateAsync();


        ApplicationUser user =
            await CreateUserAsync(
                environment.UserManager,
                "user-1",
                "user1@example.com",
                "User");


        Project project = new()
        {
            Name = "Project",
            Description = "",
            UserId = user.Id
        };

        Category category = new()
        {
            Name = "Category",
            UserId = user.Id
        };


        environment.Context.Project.Add(project);
        environment.Context.Categories.Add(category);

        await environment.Context.SaveChangesAsync();


        TaskItem taskA = new()
        {
            Title = "Task A",
            Description = "",
            DueDate = DateTime.UtcNow.AddDays(1),

            ProjectId = project.Id,
            CategoryId = category.Id
        };

        TaskItem taskB = new()
        {
            Title = "Task B",
            Description = "",
            DueDate = DateTime.UtcNow.AddDays(1),

            ProjectId = project.Id,
            CategoryId = category.Id
        };


        environment.Context.Tasks.AddRange(
            taskA,
            taskB);

        await environment.Context.SaveChangesAsync();


        int deletedCount =
            await environment.AdminService
                .DeleteTasksAsync(
                    new[] { taskA.Id });


        Assert.Equal(1, deletedCount);

        Assert.False(
            await environment.Context.Tasks
                .AnyAsync(task =>
                    task.Id == taskA.Id));

        Assert.True(
            await environment.Context.Tasks
                .AnyAsync(task =>
                    task.Id == taskB.Id));
    }


    private static async Task<ApplicationUser>
        CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            string id,
            string email,
            string role)
    {
        ApplicationUser user = new()
        {
            Id = id,
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };


        IdentityResult createResult =
            await userManager.CreateAsync(user);


        Assert.True(
            createResult.Succeeded,
            string.Join(
                "; ",
                createResult.Errors
                    .Select(error =>
                        error.Description)));


        IdentityResult roleResult =
            await userManager.AddToRoleAsync(
                user,
                role);


        Assert.True(
            roleResult.Succeeded,
            string.Join(
                "; ",
                roleResult.Errors
                    .Select(error =>
                        error.Description)));


        return user;
    }
}