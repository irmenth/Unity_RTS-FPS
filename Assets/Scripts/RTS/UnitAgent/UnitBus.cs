using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class UnitBus : MonoBehaviour
{
    [SerializeField] private GridController gc;

    private readonly List<int> keyCache = new();

    private void UpdateUnitGridPosition()
    {
        var ff = gc.CurFlowField;

        keyCache.Clear();
        keyCache.AddRange(unitReg.Keys);
        for (int i = 0; i < keyCache.Count; i++)
        {
            var key = keyCache[i];
            var agentData = unitReg[key];
            agentData.unitDgPos = ff.WorldToDirGridPos(agentData.position);
            agentData.unitOgPos = ff.WorldToObstacleGridPos(agentData.position);
            unitReg[key] = agentData;
        }
    }

    private void UpdateUnitPositionBurst()
    {
        var ff = gc.CurFlowField;

        keyCache.Clear();
        keyCache.AddRange(unitReg.Keys);
        var unitDataArray = new NativeArray<UnitAgentData>(keyCache.Count, Allocator.TempJob);
        for (int i = 0; i < keyCache.Count; i++)
        {
            var key = keyCache[i];
            unitDataArray[i] = unitReg[key];
        }

        int dgSize = ff.dgWidth * ff.dgHeight;
        var flowDir = new NativeArray<float2>(dgSize, Allocator.TempJob);
        var costMap = new NativeArray<float>(dgSize, Allocator.TempJob);
        for (int x = 0; x < ff.dgWidth; x++)
        {
            for (int y = 0; y < ff.dgHeight; y++)
            {
                flowDir[x * ff.dgHeight + y] = ff.DirGrid[x, y].direction;
                costMap[x * ff.dgHeight + y] = ff.DirGrid[x, y].cost;
            }
        }

        // int ogSize = ff.ogWidth * ff.ogHeight;
        // var unitDataList = new NativeArray<NativeArray<UnitAgentData>>(ogSize, Allocator.TempJob);
        // var obstacleList = new NativeArray<NativeArray<Obstacles>>(ogSize, Allocator.TempJob);
        // for (int x = 0; x < ff.ogWidth; x++)
        // {
        //     for (int y = 0; y < ff.ogHeight; y++)
        //     {
        //         int unitCount = ff.ObstacleGrid[x, y].unitList.Count;
        //         var agentDataArray = new NativeArray<UnitAgentData>(unitCount, Allocator.TempJob);
        //         for (int i = 0; i < unitCount; i++) agentDataArray[i] = ff.ObstacleGrid[x, y].unitList[i];

        //         int obstacleCount = ff.ObstacleGrid[x, y].obstacleList.Count;
        //         var obstacleArray = new NativeArray<Obstacles>(obstacleCount, Allocator.TempJob);
        //         for (int i = 0; i < obstacleCount; i++) obstacleArray[i] = ff.ObstacleGrid[x, y].obstacleList[i];

        //         unitDataList[x * ff.ogHeight + y] = agentDataArray;
        //         obstacleList[x * ff.ogHeight + y] = obstacleArray;
        //     }
        // }

        var job = new UnitAgentJob(ff.dgWidth, ff.dgHeight, ff.ogWidth, ff.ogHeight, Time.deltaTime, unitDataArray, flowDir, costMap);
        job.Schedule(keyCache.Count, 64).Complete();

        keyCache.Clear();
        keyCache.AddRange(unitReg.Keys);
        for (int i = 0; i < keyCache.Count; i++)
        {
            var key = keyCache[i];
            unitReg[key] = unitDataArray[i];
        }

        unitDataArray.Dispose();
        flowDir.Dispose();
        costMap.Dispose();
        // for (int x = 0; x < ff.ogWidth; x++)
        // {
        //     for (int y = 0; y < ff.ogHeight; y++)
        //     {
        //         unitDataList[x * ff.ogHeight + y].Dispose();
        //         obstacleList[x * ff.ogHeight + y].Dispose();
        //     }
        // }
        // unitDataList.Dispose();
        // obstacleList.Dispose();
    }

    private Dictionary<int, UnitAgentData> unitReg;

    private void Awake()
    {
        unitReg = UnitRegister.unitRegistry;
    }

    private void Update()
    {
        UpdateUnitGridPosition();
        UpdateUnitPositionBurst();
    }
}
