using Unity.Collections;
using UnityEngine;

public class DeleteCommand : BaseCommand
{
    public int arrayCount;
    public int[] selectedArray;

    public DeleteCommand()
    {
        commandType = CommandType.Delete;
    }
    public DeleteCommand(int[] selectedArray)
    {
        commandType = CommandType.Delete;
        this.selectedArray = selectedArray;
        arrayCount = selectedArray.Length;
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)commandType);
        writer.WriteInt(arrayCount);
        for (int i = 0; i < arrayCount; i++)
        {
            writer.WriteInt(selectedArray[i]);
        }
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
        arrayCount = reader.ReadInt();
        selectedArray ??= new int[arrayCount];
        for (int i = 0; i < arrayCount; i++)
        {
            int index = reader.ReadInt();
            selectedArray[i] = index;
        }

        Debug.Log($"[GenerateCommand Deserialize] listCount: {arrayCount}");
    }
}
