using UnityEngine;

public class FlowField
{
    public Cell[,] Grid { get; private set; }
    private readonly int gridWidth;
    private readonly int gridHeight;
    private readonly float cellRadius;
    private readonly float cellDiameter;

    public FlowField(int gridWidth, int gridHeight, float cellRadius)
    {
        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;
        this.cellRadius = cellRadius;
        this.cellDiameter = cellRadius * 2f;
        Grid = new Cell[gridWidth, gridHeight];
    }

    public void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                var worldPos = new Vector3(-0.5f * gridWidth + cellDiameter * x + cellRadius, 0, -0.5f * gridHeight + cellDiameter * y + cellRadius);
                var gridPos = new Vector2Int(x, y);
                Grid[x, y] = new Cell(worldPos, gridPos);
            }
        }
    }

    private readonly Collider[] lapBoxHitBuffter = new Collider[20];

    public void GenerateCostField(LayerMask costLayerMask, int impassibleLayer, int roughLayer)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                int hitCount = Physics.OverlapBoxNonAlloc(Grid[x, y].WorldPos, Vector3.one * cellRadius, lapBoxHitBuffter, Quaternion.identity, costLayerMask);

                bool hasRecordRough = false, hasRecordImpassible = false;
                for (int i = 0; i < hitCount; i++)
                {
                    if (Grid[x, y].Cost >= byte.MaxValue) break;

                    if (!hasRecordImpassible && lapBoxHitBuffter[i].gameObject.layer == impassibleLayer)
                    {
                        Grid[x, y].AddCost(byte.MaxValue);
                        hasRecordImpassible = true;
                    }
                    else if (!hasRecordRough && lapBoxHitBuffter[i].gameObject.layer == roughLayer)
                    {
                        Grid[x, y].AddCost(3);
                        hasRecordRough = true;
                    }
                }
            }
        }
    }
}
