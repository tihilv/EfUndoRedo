using Microsoft.EntityFrameworkCore;

namespace EfUndoable;

public abstract class DbContextUndoable : DbContext
{
    private readonly EfUndoManager _undoManager;

    protected DbContextUndoable(DbContextOptions options, EfUndoManager undoManager) : base(options)
    {
        _undoManager = undoManager;
    }

    protected DbContextUndoable(EfUndoManager undoManager)
    {
        _undoManager = undoManager;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        _undoManager?.RegisterStep(this);
        return base.SaveChangesAsync(cancellationToken);
    }

    internal Task SaveChangesAfterUndoAsync()
    {
        return base.SaveChangesAsync();        
    }
    
    public ValueTask UndoAsync()
    {
        return _undoManager.UndoAsync(this);
    }
        
    public ValueTask RedoAsync()
    {
        return _undoManager.RedoAsync(this);
    }

}