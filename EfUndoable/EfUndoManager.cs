namespace EfUndoable;

public class EfUndoManager
{
    private readonly Stack<EfUndoableStep> _undoSteps = new();
    private readonly Stack<EfUndoableStep> _redoSteps = new();

    private readonly List<IDbStrategyFactory> _dbStrategyFactories = new();

    public Boolean HasUndo => _undoSteps.Any();
    public Boolean HasRedo => _redoSteps.Any();

    public EfUndoManager()
    {
        RegisterDbStrategyFactory(new AutodetectChangesStrategyFactory());
        RegisterDbStrategyFactory(new MsSqlIdentityFixStrategyFactory());
    }
    
    public void RegisterDbStrategyFactory(IDbStrategyFactory dbStrategyFactory)
    {
        _dbStrategyFactories.Add(dbStrategyFactory);
    }

    public void RegisterStep(DbContextUndoable dbContext)
    {
        var step = new EfUndoableStep(dbContext);
        _undoSteps.Push(step);
        _redoSteps.Clear();
    }

    public async ValueTask UndoAsync(DbContextUndoable dbContext)
    {
        if (!HasUndo)
            throw new ApplicationException("There is no undo step.");

        var step = _undoSteps.Pop();
        _redoSteps.Push(step);

        var strategy = UseDbStrategiesAsync(dbContext);
        await strategy.StartStepAsync(step);
        try
        {
            await step.UndoAsync(dbContext, strategy);
            await dbContext.SaveChangesAfterUndoAsync();
        }
        finally
        {
            await strategy.StopStepAsync(step);
        }
    }

    public async ValueTask RedoAsync(DbContextUndoable dbContext)
    {
        if (!HasRedo)
            throw new ApplicationException("There is no redo step.");

        var step = _redoSteps.Pop();
        _undoSteps.Push(step);

        var strategy = UseDbStrategiesAsync(dbContext);
        await strategy.StartStepAsync(step);
        try
        {
            await step.RedoAsync(dbContext, strategy);
            await dbContext.SaveChangesAfterUndoAsync();
        }
        finally
        {
            await strategy.StopStepAsync(step);
        }
    }

    private IDbStrategy UseDbStrategiesAsync(DbContextUndoable dbContext)
    {
        CombinedDbStrategy result = new CombinedDbStrategy(_dbStrategyFactories
            .Select(f=>f.ProvideStrategy(dbContext))
            .Where(s => s != null)!);
        
        return result;
    }
}