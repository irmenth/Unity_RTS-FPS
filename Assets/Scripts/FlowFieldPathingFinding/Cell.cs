using UnityEngine;

public struct Cell
{
    public Vector3 WorldPos { get; private set; }
    public Vector2Int GridPos { get; private set; }
    public byte Cost { get; private set; }
    public ushort HeuristicCost { get; private set; }
    public Vector2 Direction { get; private set; }

    public Cell(Vector3 worldPos, Vector2Int gridPos)
    {
        WorldPos = worldPos;
        GridPos = gridPos;
        Cost = 1;
        HeuristicCost = ushort.MaxValue;
        Direction = Vector2.zero;
    }

    public void AddCost(byte amount)
    {
        if (amount >= byte.MaxValue || Cost + amount >= byte.MaxValue)
        {
            Cost = byte.MaxValue;
        }
        else
        {
            Cost += amount;
        }
    }
}
