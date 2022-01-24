using Microsoft.EntityFrameworkCore;

namespace EfUndoable.DemoApi;

public abstract class BloggingContext : DbContextUndoable
{
    protected BloggingContext(EfUndoManager undoManager) : base(undoManager)
    {
    }

    protected BloggingContext(DbContextOptions options, EfUndoManager undoManager) : base(options, undoManager)
    {
    }

    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
}