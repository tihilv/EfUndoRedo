using Microsoft.EntityFrameworkCore;

namespace EfUndoable.DemoApi;

public class BloggingContextMsSql : BloggingContext
{
    public const String DbName = "efundoable";

    public BloggingContextMsSql(EfUndoManager undoManager) : base(undoManager)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer($"Server=.;Database={DbName};Trusted_Connection=True;");
        //optionsBuilder.LogTo(Console.WriteLine);
        //optionsBuilder.EnableSensitiveDataLogging();
    }
}