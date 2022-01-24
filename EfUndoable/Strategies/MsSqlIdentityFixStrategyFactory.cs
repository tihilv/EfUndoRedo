using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EfUndoable;

internal class MsSqlIdentityFixStrategyFactory : IDbStrategyFactory
{
    public IDbStrategy? ProvideStrategy(DbContextUndoable dbContext)
    {
        if (dbContext.Database.ProviderName?.EndsWith(".SqlServer")??false)
            return new Strategy(dbContext);
        
        return null;
    }

    private class Strategy: IDbStrategy
    {
        private readonly DbContextUndoable _dbContext;
        private IDbContextTransaction _transaction;
        
        internal Strategy(DbContextUndoable dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async ValueTask StartStepAsync(EfUndoableStep step)
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync();
        }

        public async ValueTask StartOperationAsync(EfUndoableOperation operation)
        {
            var tableName = _dbContext.Model.FindEntityType(operation.Entity.GetType()).GetTableName();
            var command = $"SET IDENTITY_INSERT [{tableName}] ON";
            await _dbContext.Database.ExecuteSqlRawAsync(command);
        }

        public async ValueTask StopOperationAsync(EfUndoableOperation operation)
        {
            await _dbContext.SaveChangesAfterUndoAsync();
            var tableName = _dbContext.Model.FindEntityType(operation.Entity.GetType()).GetTableName();
            var command = $"SET IDENTITY_INSERT [{tableName}] OFF";
            await _dbContext.Database.ExecuteSqlRawAsync(command);
        }

        public async ValueTask StopStepAsync(EfUndoableStep step)
        {
            await _transaction.CommitAsync();
        }
    }
}