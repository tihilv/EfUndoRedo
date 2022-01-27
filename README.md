# EfUndoRedo
Provides undo/redo functionality for Entity Framework Core. When saving DbContext, the component generates a sequence of steps required to revert and/or replay the changes using Entity Framework change tracking mechanism.

These steps are kept in the instance of the class ``EfUndoManager`` that can be shared between different contexts of the same database.

To add Undo/Redo functionality a database context should be inherited from ``DbContextUndoable``. ``EfUndoManager`` should be passed as a constructor parameter. 

```c#
public class BloggingContext : DbContextUndoable
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
```

Usage:
```c#
var undoManager = new EfUndoManager();
using (var context = new BloggingContext(undoManager))
{
    await context.Blogs.AddAsync(new Blog());
    await context.SaveChangesAsync(); // new Blog record is added to the DB, new undo step is stored 
}

using (var context = new BloggingContext(undoManager))
{
    await context.UndoAsync(); // added Blog is removed from the DB
}

using (var context = new BloggingContext(undoManager))
{
    await context.RedoAsync(); // The Blog is restored in the DB preserving the primary key
}
```

By default, the depth of history is unlimited. To add a custom logic including the limitation of the history steps, a custom ``IStepStore<T>`` implementation can be given to ``EfUndoManager`` as a constructor parameter.

## Supported DBMS
The undo/redo mechanism was tested for the following providers: 
- SQLite (Microsoft.EntityFrameworkCore.Sqlite),
- MS SQL Server (Microsoft.EntityFrameworkCore.SqlServer)

In general, it should be no limitation of using the approach for different DBMS. If any adjustment of DBMS is required when performing undo/redo steps, it can be made using ``IDbStrategy`` implementation which should be provided by an instance of ``IDbStrategyFactory`` being registered in ``EfUndoManagerRegisterDbStrategyFactory(IDbStrategyFactory dbStrategyFactory)`` method.