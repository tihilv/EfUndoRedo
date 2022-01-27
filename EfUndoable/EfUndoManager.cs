namespace EfUndoable;

public class EfUndoManager
{
    private readonly IStepStore<EfUndoableStep> _stepStore;
    private readonly List<IDbStrategyFactory> _dbStrategyFactories = new();

    public bool HasUndo => _stepStore.HasUndo;
    public bool HasRedo => _stepStore.HasRedo;

    public EfUndoManager(): this(new DefaultStepStore<EfUndoableStep>())
    {
    }

    public EfUndoManager(IStepStore<EfUndoableStep> stepStore)
    {
        _stepStore = stepStore;
        RegisterDbStrategyFactory(new AutodetectChangesStrategyFactory());
        RegisterDbStrategyFactory(new MsSqlIdentityFixStrategyFactory());
    }
    
    public void RegisterDbStrategyFactory(IDbStrategyFactory dbStrategyFactory)
    {
        _dbStrategyFactories.Add(dbStrategyFactory);
    }

    internal void RegisterStep(DbContextUndoable dbContext)
    {
        var step = new EfUndoableStep(dbContext);
        _stepStore.RegisterStep(step);
    }

    internal async ValueTask UndoAsync(DbContextUndoable dbContext)
    {
        if (!HasUndo)
            throw new ApplicationException("There is no undo step.");

        var step = _stepStore.GetUndoStep();

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

    internal async ValueTask RedoAsync(DbContextUndoable dbContext)
    {
        if (!HasRedo)
            throw new ApplicationException("There is no redo step.");

        var step = _stepStore.GetRedoStep();

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