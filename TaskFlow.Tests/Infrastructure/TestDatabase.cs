using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Web.Data;

namespace TaskFlow.Tests.Infrastructure;

public sealed class TestDatabase : IAsyncDisposable
{
    public SqliteConnection Connection { get; }

    public AppDbContext Context { get; }


    private TestDatabase(
        SqliteConnection connection,
        AppDbContext context)
    {
        Connection = connection;
        Context = context;
    }


    public static async Task<TestDatabase> CreateAsync()
    {
        SqliteConnection connection =
            new("Data Source=:memory:");

        await connection.OpenAsync();


        DbContextOptions<AppDbContext> options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;


        AppDbContext context =
            new(options);


        await context.Database.EnsureCreatedAsync();


        return new TestDatabase(
            connection,
            context);
    }


    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await Connection.DisposeAsync();
    }
}