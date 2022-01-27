namespace EfUndoable;

public interface IDbStrategyFactory
{
    IDbStrategy? ProvideStrategy(DbContextUndoable dbContext);
}