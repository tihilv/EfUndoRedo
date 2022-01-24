using Microsoft.EntityFrameworkCore;

namespace EfUndoable;

public sealed class EfUndoableStep
{
    private readonly List<EfUndoableOperation> _operations = new();
    private bool _undone;
        
    public EfUndoableStep(DbContextUndoable dbContext)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    _operations.Add(new EfUndoableOperationAdded(entry.Entity));
                    break;
                case EntityState.Deleted:
                    _operations.Add(new EfUndoableOperationDeleted(entry.Entity));
                    break;
                case EntityState.Modified:
                    _operations.Add(new EfUndoableOperationModified(entry.Entity, entry.GetDatabaseValues(), entry.CurrentValues));
                    break;
            }
        }
    }

    public async ValueTask UndoAsync(DbContextUndoable dbContext, IDbStrategy strategy)
    {
        if (_undone)
            throw new ApplicationException("The undo for the step is already applied.");

        for (int i = _operations.Count - 1; i >= 0; i--)
        {
            var operation = _operations[i];
            await strategy.StartOperationAsync(operation);
            try
            {
                await operation.UndoAsync(dbContext);
            }
            finally
            {
                await strategy.StopOperationAsync(operation);
            }
        }

        _undone = true;
    }

    public async ValueTask RedoAsync(DbContextUndoable dbContext, IDbStrategy strategy)
    {
        if (!_undone)
            throw new ApplicationException("The step is already applied.");

        for (int i = 0; i < _operations.Count; i++)
        {
            var operation = _operations[i];
            await strategy.StartOperationAsync(operation);
            try
            {
                await operation.RedoAsync(dbContext);
            }
            finally
            {
                await strategy.StopOperationAsync(operation);
            }
        }

        _undone = false;
    }
}