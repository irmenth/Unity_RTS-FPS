using System.Collections.Generic;

public struct CommandsReadyEvent
{
    public List<BaseCommand> commands;

    public CommandsReadyEvent(List<BaseCommand> commands)
    {
        this.commands = commands;
    }
}
