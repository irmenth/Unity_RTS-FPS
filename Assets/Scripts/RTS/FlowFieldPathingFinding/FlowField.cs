using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class FlowField
{
    public DirCell[,] DirGrid { get; private set; }
    public ObstacleCell[,] ObstacleGrid { get; private set; }
    public readonly int dgWidth, dgHeight, ogWidth, ogHeight;
    public readonly float dcRadius, dcDiameter, ocRadius, ocDiameter;

    /// <summary>
    /// </summary>
    /// <param name="dgWidth">
    /// Width of the DirGrid
    /// </param>
    /// <param name="dgHeight">
    /// Height of the DirGrid
    /// </param>
    /// <param name="dcRadius">
    /// Radius of the DirCell
    /// </param>
    /// <param name="ogWidth">
    /// Width of the ObstacleGrid
    /// </param>
    /// <param name="ogHeight">
    /// Height of the ObstacleGrid
    /// </param>
    /// <param name="ocRadius">
    /// Radius of the ObstacleCell
    /// </param>
    public FlowField(int dgWidth, int dgHeight, float dcRadius, int ogWidth, int ogHeight, float ocRadius)
    {
        this.dgWidth = dgWidth;
        this.dgHeight = dgHeight;
        this.dcRadius = dcRadius;
        dcDiameter = dcRadius * 2f;
        DirGrid = new DirCell[dgWidth, dgHeight];

        this.ogWidth = ogWidth;
        this.ogHeight = ogHeight;
        this.ocRadius = ocRadius;
        ocDiameter = ocRadius * 2f;
        ObstacleGrid = new ObstacleCell[ogWidth, ogHeight];
    }

    /// <summary>
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns>
    /// -1 * Vector2Int.one if out of range
    /// </returns>
    public int2 WorldToDirGridPos(float2 worldPos)
    {
        int2 gridPos = new((int)math.floor(worldPos.x / dcDiameter), (int)math.floor(worldPos.y / dcDiameter));
        if (gridPos.x < 0 || gridPos.x >= dgWidth || gridPos.y < 0 || gridPos.y >= dgHeight) return new int2(-1, -1);
        return gridPos;
    }

    /// <summary>
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns>
    /// -1 * Vector2Int.one if out of range
    /// </returns>
    public int2 WorldToObstacleGridPos(float2 worldPos)
    {
        int2 gridPos = new((int)math.floor(worldPos.x / ocDiameter), (int)math.floor(worldPos.y / ocDiameter));
        if (gridPos.x < 0 || gridPos.x >= ogWidth || gridPos.y < 0 || gridPos.y >= ogHeight) return new int2(-1, -1);
        return gridPos;
    }

    public void GenerateGrid()
    {
        for (int x = 0; x < dgWidth; x++)
        {
            for (int y = 0; y < dgHeight; y++)
            {
                Vector3 worldPos = new(dcDiameter * x + dcRadius, 0, dcDiameter * y + dcRadius);
                Vector2Int gridPos = new(x, y);
                DirGrid[x, y] = new DirCell(worldPos, gridPos);
            }
        }

        for (int x = 0; x < ogWidth; x++)
        {
            for (int y = 0; y < ogHeight; y++)
            {
                Vector3 worldPos = new(ocDiameter * x + ocRadius, 0, ocDiameter * y + ocRadius);
                Vector2Int gridPos = new(x, y);
                ObstacleGrid[x, y] = new ObstacleCell(worldPos, gridPos);
            }
        }
    }

    private readonly Collider[] cfBoxHitBuffter = new Collider[10];

    public void GenerateCostField(LayerMask costLayerMask, int impassibleLayer, int roughLayer)
    {
        float subCellDiameter = dcDiameter / 3f;
        for (int x = 0; x < dgWidth; x++)
        {
            for (int y = 0; y < dgHeight; y++)
            {
                int hitCount = Physics.OverlapBoxNonAlloc(DirGrid[x, y].GetWorldPos(), Vector3.one * dcRadius, cfBoxHitBuffter, Quaternion.identity, costLayerMask);

                bool hasRecordRough = false, hasRecordImpassible = false;
                for (int i = 0; i < hitCount; i++)
                {
                    if (float.IsInfinity(DirGrid[x, y].cost)) continue;

                    if (!hasRecordImpassible && cfBoxHitBuffter[i].gameObject.layer == impassibleLayer)
                    {
                        int subCellHitCount = 0;
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if (subCellHitCount >= 4) break;

                                var curSubCellPos = DirGrid[x, y].GetWorldPos() + new Vector3(dx * subCellDiameter, -10, dy * subCellDiameter);
                                if (Physics.Raycast(curSubCellPos, Vector3.up, out var hit, 100f, 1 << impassibleLayer))
                                    subCellHitCount++;
                            }
                        }

                        if (subCellHitCount >= 4)
                        {
                            DirGrid[x, y].cost += float.PositiveInfinity;
                            hasRecordImpassible = true;
                        }
                    }
                    else if (!hasRecordRough && cfBoxHitBuffter[i].gameObject.layer == roughLayer)
                    {
                        int subCellHitCount = 0;
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if (subCellHitCount >= 4) break;

                                var curSubCellPos = DirGrid[x, y].GetWorldPos() + new Vector3(dx * subCellDiameter, -10, dy * subCellDiameter);
                                if (Physics.Raycast(curSubCellPos, Vector3.up, out var hit, 100f, 1 << roughLayer))
                                    subCellHitCount++;
                            }
                        }

                        if (subCellHitCount >= 4)
                        {
                            DirGrid[x, y].cost += 1f;
                            hasRecordRough = true;
                        }
                    }
                }
            }
        }
    }

    public void GenerateHeatMapBurst(int2 destinationGridPos)
    {
        int size = dgWidth * dgHeight, destination = destinationGridPos.x * dgHeight + destinationGridPos.y;

        var costMap = new NativeArray<float>(size, Allocator.TempJob);
        var heatMap = new NativeArray<float>(size, Allocator.TempJob);
        var openList = new NativeQueue<int>(Allocator.TempJob);
        var inOpenList = new NativeArray<byte>(size, Allocator.TempJob);
        var closeList = new NativeArray<byte>(size, Allocator.TempJob);

        for (int i = 0; i < dgWidth; i++)
        {
            for (int j = 0; j < dgHeight; j++)
            {
                costMap[i * dgHeight + j] = DirGrid[i, j].cost;
            }
        }

        var job = new HeatMapJob(dgWidth, dgHeight, destination, openList, inOpenList, closeList, costMap, heatMap);
        job.Schedule().Complete();

        for (int i = 0; i < dgWidth; i++)
        {
            for (int j = 0; j < dgHeight; j++)
            {
                DirGrid[i, j].heat = heatMap[i * dgHeight + j];
            }
        }

        costMap.Dispose();
        heatMap.Dispose();
        openList.Dispose();
        inOpenList.Dispose();
        closeList.Dispose();
    }

    public void GenerateFlowFieldBurst()
    {
        int size = dgWidth * dgHeight;

        var heatMap = new NativeArray<float>(size, Allocator.TempJob);
        var flowDir = new NativeArray<float2>(size, Allocator.TempJob);

        for (int i = 0; i < dgWidth; i++)
        {
            for (int j = 0; j < dgHeight; j++)
            {
                heatMap[i * dgHeight + j] = DirGrid[i, j].heat;
            }
        }

        var job = new FlowFieldJob(dgWidth, dgHeight, heatMap, flowDir);
        job.Schedule(size, 64).Complete();

        for (int i = 0; i < dgWidth; i++)
        {
            for (int j = 0; j < dgHeight; j++)
            {
                DirGrid[i, j].direction = flowDir[i * dgHeight + j];
            }
        }

        heatMap.Dispose();
        flowDir.Dispose();
    }

    private readonly Collider[] omBoxHitBuffer = new Collider[10];
    private readonly Dictionary<Collider, Obstacles> colliderBuffer = new();

    public void GenerateObstacleMap(int impassibleLayer)
    {
        for (int x = 0; x < ogWidth; x++)
        {
            for (int y = 0; y < ogHeight; y++)
            {
                int hitCount = Physics.OverlapBoxNonAlloc(ObstacleGrid[x, y].GetWorldPos(), Vector3.one * ocRadius, omBoxHitBuffer, Quaternion.identity, 1 << impassibleLayer);

                for (int i = 0; i < hitCount; i++)
                {
                    var collider = omBoxHitBuffer[i];

                    if (!colliderBuffer.ContainsKey(collider))
                        colliderBuffer[collider] = collider.GetComponent<ObstacleCollider>().obstacle;

                    var obstacleCollider = colliderBuffer[collider];

                    if (!ObstacleGrid[x, y].obstacleList.Contains(obstacleCollider))
                        ObstacleGrid[x, y].obstacleList.Add(obstacleCollider);
                }
            }
        }
    }
}
