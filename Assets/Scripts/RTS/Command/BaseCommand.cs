using Unity.Collections;

public class BaseCommand
{
    public CommandType commandType = CommandType.None;

    public virtual void Serialize(ref DataStreamWriter writer) { }

    public virtual void Deserialize(ref DataStreamReader reader) { }
}
