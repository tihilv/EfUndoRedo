using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EfUndoable;

internal sealed class EfUndoableOperationModified:EfUndoableOperation
{
    private readonly Dictionary<String, Object> _originalValues = new();
    private readonly Dictionary<String, Object> _currentValues = new();

    public EfUndoableOperationModified(object entity, PropertyValues originalValues, PropertyValues currentValues) : base(entity)
    {
        var properties = originalValues.Properties.Union(currentValues.Properties).Distinct();
        foreach (var property in properties)
        {
            var name = property.Name;
            var originalValue = originalValues[name];
            var currentValue = currentValues[name];
            if (!((originalValue == null && currentValue == null) || (originalValue?.Equals(currentValue)??false)))
            {
                _originalValues.Add(name, originalValue);
                _currentValues.Add(name, currentValue);
            }
        }
    }

    public override Task UndoAsync(DbContext dbContext)
    {
        dbContext.Attach(Entity);
        dbContext.Entry(Entity).CurrentValues.SetValues(_originalValues);
        return Task.CompletedTask;
    }

    public override Task RedoAsync(DbContext dbContext)
    {
        dbContext.Attach(Entity);
        dbContext.Entry(Entity).CurrentValues.SetValues(_currentValues);
        return Task.CompletedTask;
    }
}