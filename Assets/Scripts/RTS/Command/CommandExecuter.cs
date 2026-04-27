using UnityEngine;

public class CommandExecuter : MonoBehaviour
{
    public static CommandExecuter instance;

    private void OnCommandsReady(CommandsReadyEvent e)
    {
        if (!UnitBus.instance) return;
        foreach (var cmd in e.commands)
        {
            switch (cmd.commandType)
            {
                case CommandType.Generate:
                    UnitBus.instance.InstantiateUnit(cmd as GenerateCommand);
                    break;
                case CommandType.Move:
                    UnitBus.instance.SetDestination(cmd as MoveCommand);
                    break;
                case CommandType.Delete:
                    UnitBus.instance.Delete(cmd as DeleteCommand);
                    break;
            }
        }
    }

    private void Start()
    {
        instance = this;
        EventBus.Subscribe<CommandsReadyEvent>(OnCommandsReady);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<CommandsReadyEvent>(OnCommandsReady);
        instance = null;
    }
}
