using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class MoveCommand : BaseCommand
{
    public float2 pos;
    public int arrayCount;
    public int[] selectedArray;

    public MoveCommand()
    {
        commandType = CommandType.Move;
    }
    public MoveCommand(float2 pos, int[] selectedArray)
    {
        commandType = CommandType.Move;
        this.pos = pos;
        this.selectedArray = selectedArray;
        arrayCount = selectedArray.Length;
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)commandType);
        writer.WriteFloat(pos.x);
        writer.WriteFloat(pos.y);
        writer.WriteInt(arrayCount);
        for (int i = 0; i < arrayCount; i++)
        {
            writer.WriteInt(selectedArray[i]);
        }
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
        pos.x = reader.ReadFloat();
        pos.y = reader.ReadFloat();
        arrayCount = reader.ReadInt();
        selectedArray ??= new int[arrayCount];
        for (int i = 0; i < arrayCount; i++)
        {
            int index = reader.ReadInt();
            selectedArray[i] = index;
        }

        Debug.Log($"[MoveCommand Deserialize] pos: {pos} listCount: {arrayCount}");
    }
}
