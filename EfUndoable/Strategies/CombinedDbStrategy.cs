namespace EfUndoable;

public class CombinedDbStrategy: IDbStrategy
{
    private readonly IDbStrategy[] _items;
    
    public CombinedDbStrategy(IEnumerable<IDbStrategy> items)
    {
        _items = items.ToArray();
    }
    
    public async ValueTask StartStepAsync(EfUndoableStep step)
    {
        foreach (var item in _items)
            await item.StartStepAsync(step);
    }

    public async ValueTask StartOperationAsync(EfUndoableOperation operation)
    {
        foreach (var item in _items)
            await item.StartOperationAsync(operation);
    }

    public async ValueTask StopOperationAsync(EfUndoableOperation operation)
    {
        foreach (var item in _items)
            await item.StopOperationAsync(operation);
    }

    public async ValueTask StopStepAsync(EfUndoableStep step)
    {
        foreach (var item in _items)
            await item.StopStepAsync(step);
    }
}