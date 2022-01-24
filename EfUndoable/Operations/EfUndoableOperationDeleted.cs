using Microsoft.EntityFrameworkCore;

namespace EfUndoable;

internal sealed class EfUndoableOperationDeleted:EfUndoableOperation
{
    public EfUndoableOperationDeleted(object entity) : base(entity)
    {
    }

    public override async Task UndoAsync(DbContext dbContext)
    {
        dbContext.Attach(Entity);
        await dbContext.AddAsync(Entity);
    }

    public override Task RedoAsync(DbContext dbContext)
    {
        dbContext.Remove(Entity);
        return Task.CompletedTask;
    }
}