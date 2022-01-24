using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EfUndoable.DemoApi;

public class BloggingContextSqlite : BloggingContext
{
    public const String FileName = "blogging.db";

    public BloggingContextSqlite(EfUndoManager undoManager) : base(undoManager)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={FileName}");
    }
}

public class BloggingContextSqliteInMemory : BloggingContext
{
    public static SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }

    public static BloggingContextSqliteInMemory Create(SqliteConnection connection, EfUndoManager undoManager)
    {
        DbContextOptions<BloggingContextSqliteInMemory> options = new DbContextOptionsBuilder<BloggingContextSqliteInMemory>()
            .UseSqlite(connection) // Set the connection explicitly, so it won't be closed automatically by EF
            .Options;

        var result = new BloggingContextSqliteInMemory(options, undoManager);
        result.Database.EnsureCreated();
        return result;
    }
    
    internal BloggingContextSqliteInMemory(DbContextOptions<BloggingContextSqliteInMemory> options, EfUndoManager undoManager) : base(options, undoManager)
    {
    }
}