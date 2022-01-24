namespace EfUndoable;

public interface IDbStrategyFactory
{
    IDbStrategy? ProvideStrategy(DbContextUndoable dbContext);
}

public interface IDbStrategy
{
    ValueTask StartStepAsync(EfUndoableStep step);
    ValueTask StartOperationAsync(EfUndoableOperation operation);
    ValueTask StopOperationAsync(EfUndoableOperation operation);
    ValueTask StopStepAsync(EfUndoableStep step);
}