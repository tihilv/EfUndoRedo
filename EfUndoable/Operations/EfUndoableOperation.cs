using Microsoft.EntityFrameworkCore;

namespace EfUndoable;

public abstract class EfUndoableOperation
{
    internal readonly Object Entity;

    protected EfUndoableOperation(Object entity)
    {
        Entity = entity;
    }

    public abstract Task UndoAsync(DbContext dbContext);
    public abstract Task RedoAsync(DbContext dbContext);
}