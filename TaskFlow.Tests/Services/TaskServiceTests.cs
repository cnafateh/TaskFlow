using Microsoft.EntityFrameworkCore;
using TaskFlow.Tests.Infrastructure;
using TaskFlow.Web.Data;
using TaskFlow.Web.Models;
using TaskFlow.Web.Services;
using TaskFlow.Web.ViewModels.Tasks;

namespace TaskFlow.Tests.Services;

public class TaskServiceTests
{
    [Fact]
    public async Task GetByIdAsync_WhenTaskBelongsToUser_ReturnsTask()
    {
        await using TestDatabase database =
            await TestDatabase.CreateAsync();

        TestData data =
            await SeedDataAsync(database.Context);

        TaskService service =
            new(database.Context);


        TaskItem? result =
            await service.GetByIdAsync(
                data.TaskA.Id,
                data.UserA.Id);


        Assert.NotNull(result);
        Assert.Equal(data.TaskA.Id, result.Id);
    }


    [Fact]
    public async Task GetByIdAsync_WhenTaskBelongsToAnotherUser_ReturnsNull()
    {
        await using TestDatabase database =
            await TestDatabase.CreateAsync();

        TestData data =
            await SeedDataAsync(database.Context);

        TaskService service =
            new(database.Context);


        TaskItem? result =
            await service.GetByIdAsync(
                data.TaskB.Id,
                data.UserA.Id);


        Assert.Null(result);
    }


    [Fact]
    public async Task CreateAsync_WhenProjectBelongsToAnotherUser_ReturnsFalse()
    {
        await using TestDatabase database =
            await TestDatabase.CreateAsync();

        TestData data =
            await SeedDataAsync(database.Context);

        TaskService service =
            new(database.Context);


        TaskItem task = new()
        {
            Title = "Unauthorized task",
            Description = "Test",
            DueDate = DateTime.UtcNow.AddDays(1),

            ProjectId = data.ProjectB.Id,
            CategoryId = data.CategoryA.Id
        };


        bool result =
            await service.CreateAsync(
                task,
                data.UserA.Id);


        Assert.False(result);

        Assert.False(
            await database.Context.Tasks
                .AnyAsync(task =>
                    task.Title == "Unauthorized task"));
    }


    [Fact]
    public async Task CreateAsync_WhenCategoryBelongsToAnotherUser_ReturnsFalse()
    {
        await using TestDatabase database =
            await TestDatabase.CreateAsync();

        TestData data =
            await SeedDataAsync(database.Context);

        TaskService service =
            new(database.Context);


        TaskItem task = new()
        {
            Title = "Unauthorized category task",
            Description = "Test",
            DueDate = DateTime.UtcNow.AddDays(1),

            ProjectId = data.ProjectA.Id,
            CategoryId = data.CategoryB.Id
        };


        bool result =
            await service.CreateAsync(
                task,
                data.UserA.Id);


        Assert.False(result);

        Assert.False(
            await database.Context.Tasks
                .AnyAsync(task =>
                    task.Title ==
                    "Unauthorized category task"));
    }


    [Fact]
    public async Task UpdateAsync_WhenTaskBelongsToUser_UpdatesTask()
    {
        await using TestDatabase database =
            await TestDatabase.CreateAsync();

        TestData data =
            await SeedDataAsync(database.Context);

        TaskService service =
            new(database.Context);


        TaskItem update = new()
        {
            Id = data.TaskA.Id,

            Title = "Updated task",
            Description = "Updated description",
            DueDate = DateTime.UtcNow.AddDays(5),

            ProjectId = data.ProjectA.Id,
            CategoryId = data.CategoryA.Id
        };


        bool result =
            await service.UpdateAsync(
                update,
                data.UserA.Id);


        Assert.True(result);


        database.Context.ChangeTracker.Clear();


        TaskItem updatedTask =
            await database.Context.Tasks
                .SingleAsync(task =>
                    task.Id == data.TaskA.Id);


        Assert.Equal(
            "Updated task",
            updatedTask.Title);

        Assert.NotNull(
            updatedTask.UpdatedAt);
    }


    [Fact]
    public async Task UpdateAsync_WhenTaskBelongsToAnotherUser_ReturnsFalse()
    {
        await using TestDatabase database =
            await TestDatabase.CreateAsync();

        TestData data =
            await SeedDataAsync(database.Context);

        TaskService service =
            new(database.Context);


        TaskItem update = new()
        {
            Id = data.TaskB.Id,

            Title = "Should not update",
            Description = "Test",
            DueDate = DateTime.UtcNow.AddDays(2),

            ProjectId = data.ProjectA.Id,
            CategoryId = data.CategoryA.Id
        };


        bool result =
            await service.UpdateAsync(
                update,
                data.UserA.Id);


        Assert.False(result);


        database.Context.ChangeTracker.Clear();


        TaskItem originalTask =
            await database.Context.Tasks
                .SingleAsync(task =>
                    task.Id == data.TaskB.Id);


        Assert.Equal(
            "Task B",
            originalTask.Title);
    }


    [Fact]
    public async Task DeleteAsync_WhenTaskBelongsToUser_DeletesTask()
    {
        await using TestDatabase database =
            await TestDatabase.CreateAsync();

        TestData data =
            await SeedDataAsync(database.Context);

        TaskService service =
            new(database.Context);


        bool result =
            await service.DeleteAsync(
                data.TaskA.Id,
                data.UserA.Id);


        Assert.True(result);

        Assert.False(
            await database.Context.Tasks
                .AnyAsync(task =>
                    task.Id == data.TaskA.Id));
    }


    [Fact]
    public async Task DeleteAsync_WhenTaskBelongsToAnotherUser_ReturnsFalse()
    {
        await using TestDatabase database =
            await TestDatabase.CreateAsync();

        TestData data =
            await SeedDataAsync(database.Context);

        TaskService service =
            new(database.Context);


        bool result =
            await service.DeleteAsync(
                data.TaskB.Id,
                data.UserA.Id);


        Assert.False(result);

        Assert.True(
            await database.Context.Tasks
                .AnyAsync(task =>
                    task.Id == data.TaskB.Id));
    }

    [Fact]
    public async Task CreateAsync_WhenProjectAndCategoryBelongToUser_CreatesTask()
    {
        await using TestDatabase database =
            await TestDatabase.CreateAsync();

        TestData data =
            await SeedDataAsync(database.Context);

        TaskService service =
            new(database.Context);


        TaskItem task = new()
        {
            Title = "New Task",
            Description = "Valid task",
            DueDate = DateTime.UtcNow.AddDays(2),

            ProjectId = data.ProjectA.Id,
            CategoryId = data.CategoryA.Id
        };


        bool result =
            await service.CreateAsync(
                task,
                data.UserA.Id);


        Assert.True(result);

        Assert.True(
            await database.Context.Tasks
                .AnyAsync(existing =>
                    existing.Title == "New Task"));
    }

    [Fact]
    public async Task GetFilteredAsync_ReturnsOnlyCurrentUsersTasks()
    {
        await using TestDatabase database =
            await TestDatabase.CreateAsync();

        TestData data =
            await SeedDataAsync(database.Context);

        TaskService service =
            new(database.Context);


        TaskFilter filter = new()
        {
            Page = 1,
            PageSize = 10
        };


        PagedResult<TaskItem> result =
            await service.GetFilteredAsync(
                filter,
                data.UserA.Id);


        Assert.Single(result.Items);

        Assert.Equal(
            data.TaskA.Id,
            result.Items[0].Id);

        Assert.DoesNotContain(
            result.Items,
            task => task.Id == data.TaskB.Id);
    }

    [Fact]
    public async Task GetSummaryAsync_CountsOnlyCurrentUsersTasks()
    {
        await using TestDatabase database =
            await TestDatabase.CreateAsync();

        TestData data =
            await SeedDataAsync(database.Context);


        data.TaskA.Status =
            TaskFlow.Web.Models.Enums.TaskStatus.InProgress;


        TaskItem completedTask = new()
        {
            Title = "Completed Task",
            Description = "",
            DueDate = DateTime.UtcNow,

            ProjectId = data.ProjectA.Id,
            CategoryId = data.CategoryA.Id,

            Status =
                TaskFlow.Web.Models.Enums.TaskStatus.Done
        };


        database.Context.Tasks.Add(completedTask);

        await database.Context.SaveChangesAsync();


        TaskService service =
            new(database.Context);


        TaskSummaryViewModel result =
            await service.GetSummaryAsync(
                data.UserA.Id);


        Assert.Equal(2, result.TotalTasks);
        Assert.Equal(1, result.InProgressTasks);
        Assert.Equal(1, result.CompletedTasks);
    }


    private static async Task<TestData> SeedDataAsync(
        AppDbContext context)
    {
        ApplicationUser userA = new()
        {
            Id = "user-a",
            UserName = "usera@example.com",
            Email = "usera@example.com"
        };

        ApplicationUser userB = new()
        {
            Id = "user-b",
            UserName = "userb@example.com",
            Email = "userb@example.com"
        };


        context.Users.AddRange(
            userA,
            userB);

        await context.SaveChangesAsync();


        Project projectA = new()
        {
            Name = "Project A",
            Description = "",
            UserId = userA.Id
        };

        Project projectB = new()
        {
            Name = "Project B",
            Description = "",
            UserId = userB.Id
        };


        Category categoryA = new()
        {
            Name = "Category A",
            UserId = userA.Id
        };

        Category categoryB = new()
        {
            Name = "Category B",
            UserId = userB.Id
        };


        context.Project.AddRange(
            projectA,
            projectB);

        context.Categories.AddRange(
            categoryA,
            categoryB);

        await context.SaveChangesAsync();


        TaskItem taskA = new()
        {
            Title = "Task A",
            Description = "",
            DueDate =
                DateTime.UtcNow.AddDays(1),

            ProjectId = projectA.Id,
            CategoryId = categoryA.Id
        };

        TaskItem taskB = new()
        {
            Title = "Task B",
            Description = "",
            DueDate =
                DateTime.UtcNow.AddDays(1),

            ProjectId = projectB.Id,
            CategoryId = categoryB.Id
        };


        context.Tasks.AddRange(
            taskA,
            taskB);

        await context.SaveChangesAsync();


        return new TestData(
            userA,
            userB,
            projectA,
            projectB,
            categoryA,
            categoryB,
            taskA,
            taskB);
    }


    private sealed record TestData(
        ApplicationUser UserA,
        ApplicationUser UserB,
        Project ProjectA,
        Project ProjectB,
        Category CategoryA,
        Category CategoryB,
        TaskItem TaskA,
        TaskItem TaskB);
}