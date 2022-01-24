namespace EfUndoable;

internal class AutodetectChangesStrategyFactory : IDbStrategyFactory
{
    public IDbStrategy? ProvideStrategy(DbContextUndoable dbContext)
    {
        if (dbContext.ChangeTracker.AutoDetectChangesEnabled)
            return new Strategy(dbContext);
        
        return null;
    }

    private class Strategy: IDbStrategy
    {
        private readonly DbContextUndoable _dbContext;
        
        internal Strategy(DbContextUndoable dbContext)
        {
            _dbContext = dbContext;
        }

        public ValueTask StartStepAsync(EfUndoableStep step)
        {
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            return ValueTask.CompletedTask;
        }

        public ValueTask StartOperationAsync(EfUndoableOperation operation)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask StopOperationAsync(EfUndoableOperation operation)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask StopStepAsync(EfUndoableStep step)
        {
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            return ValueTask.CompletedTask;
        }
    }
}