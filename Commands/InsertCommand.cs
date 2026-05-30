using System;
using System.Linq;

namespace AlchemyRPG;

public class InsertCommand : ICommand
{
    private readonly Guid _itemIdToInsert;
    private readonly Guid _targetContainerId;

    public InsertCommand(Guid itemIdToInsert, Guid targetContainerId)
    {
        _itemIdToInsert = itemIdToInsert;
        _targetContainerId = targetContainerId;
    }

    public bool CanExecute(GameState state, Player executor)
    {
        return !executor.IsDead;
    }

    public void Execute(GameState state, Player executor)
    {
        bool success = executor.TryInsertItem(_itemIdToInsert, _targetContainerId);

        if (success)
        {
            state.SystemLogs.Notify(new SystemLogData(LogType.Loot, $"{executor.Name} successfully slotted an item."));
            state.EventLog.Push($"{executor.Name} slotted an item!");
        }
        else
        {
            executor.SetLogMessage("Cannot insert item into this container.");
        }
    }
}