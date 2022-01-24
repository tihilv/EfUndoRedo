using Microsoft.EntityFrameworkCore;

namespace EfUndoable;

internal sealed class EfUndoableOperationAdded:EfUndoableOperation
{
    public EfUndoableOperationAdded(object entity) : base(entity)
    {
    }

    public override Task UndoAsync(DbContext dbContext)
    {
        dbContext.Remove(Entity);
        return Task.CompletedTask;
    }

    public override async Task RedoAsync(DbContext dbContext)
    {
        dbContext.Attach(Entity);
        await dbContext.AddAsync(Entity);
    }
}