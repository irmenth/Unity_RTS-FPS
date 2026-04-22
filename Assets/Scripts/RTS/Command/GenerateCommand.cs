using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class GenerateCommand : BaseCommand
{
    public UnitType unitType;
    public int count;
    public float2 pos;

    public GenerateCommand()
    {
        commandType = CommandType.Generate;
    }
    public GenerateCommand(UnitType unitType, int count, float2 pos)
    {
        this.unitType = unitType;
        this.count = count;
        this.pos = pos;
        commandType = CommandType.Generate;
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)commandType);
        writer.WriteByte((byte)unitType);
        writer.WriteInt(count);
        writer.WriteFloat(pos.x);
        writer.WriteFloat(pos.y);
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
        unitType = (UnitType)reader.ReadByte();
        count = reader.ReadInt();
        pos.x = reader.ReadFloat();
        pos.y = reader.ReadFloat();

        Debug.Log($"[GenerateCommand Deserialize] unitType: {unitType}, count: {count}, pos: {pos}");
    }
}
