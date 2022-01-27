namespace EfUndoable;

public interface IStepStore<T>
{
    /// <summary>
    /// Is at least one undo step available?
    /// </summary>
    bool HasUndo { get; }
    
    /// <summary>
    /// Is at least one redo step available?
    /// </summary>
    bool HasRedo { get; }

    /// <summary>
    /// Registers a new history step. By convention, all available redo steps should be cleared
    /// </summary>
    /// <param name="step">New history step to register</param>
    void RegisterStep(T step);

    /// <summary>
    /// Returns the most recent undo step moving it to the redo collection as the next redo step
    /// </summary>
    public T GetUndoStep();
    /// <summary>
    /// Returns next redo step moving it to the undo collection as the recent undo step
    /// </summary>
    public T GetRedoStep();
}

public class DefaultStepStore<T>: IStepStore<T>
{
    private readonly Stack<T> _undoSteps = new();
    private readonly Stack<T> _redoSteps = new();
    
    public bool HasUndo => _undoSteps.Any();
    public bool HasRedo => _redoSteps.Any();
    
    public void RegisterStep(T step)
    {
        _undoSteps.Push(step);
        _redoSteps.Clear();
    }

    public T GetUndoStep()
    {
        var step = _undoSteps.Pop();
        _redoSteps.Push(step);
        return step;
    }

    public T GetRedoStep()
    {
        var step = _redoSteps.Pop();
        _undoSteps.Push(step);
        return step;
    }
}