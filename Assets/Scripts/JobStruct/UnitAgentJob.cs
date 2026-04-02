using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct UnitAgentJob : IJobParallelFor
{
    private readonly int dgHeight, dgWidth, ogWidht, ogHeight;
    private readonly float deltaTime;
    private NativeArray<UnitAgentData> agentDatas;
    [ReadOnly] private NativeArray<float2> flowDir;
    [ReadOnly] private NativeArray<float> costMap;
    // [ReadOnly] private NativeArray<NativeArray<UnitAgentData>> unitDataList;
    // [ReadOnly] private NativeArray<NativeArray<Obstacles>> obstacleList;

    public UnitAgentJob(int dgWidth, int dgHeight, int ogWidht, int ogHeight, float deltaTime, NativeArray<UnitAgentData> agentDatas, NativeArray<float2> flowDir, NativeArray<float> costMap)
    {
        this.dgWidth = dgWidth;
        this.dgHeight = dgHeight;
        this.ogWidht = ogWidht;
        this.ogHeight = ogHeight;
        this.deltaTime = deltaTime;
        this.agentDatas = agentDatas;
        this.flowDir = flowDir;
        this.costMap = costMap;
        // this.unitDataList = unitDataList;
        // this.obstacleList = obstacleList;
    }

    public void Execute(int index)
    {
        // FlowFieldVelocity();
        UnitAgentData agentData = agentDatas[index];
        int dgIndex = agentData.unitDgPos.x * dgHeight + agentData.unitDgPos.y;
        float cost = costMap[dgIndex];
        float curMaxSpeed = math.isinf(cost) ? agentData.moveSpeed : agentData.moveSpeed / cost;
        float2 dir = flowDir[dgIndex];
        if (!UsefulUtils.Approximately(dir, new float2(-1, -1)) && !UsefulUtils.Approximately(dir, float2.zero))
        {
            agentData.velocity += 4f * curMaxSpeed * deltaTime * dir;
            agentData.velocity = curMaxSpeed * math.normalize(agentData.velocity);
        }

        // // BoidsVelocityCorrection();
        // Random rand = new((uint)index);
        // float2 offsetSum = new(0, 0);
        // int count = 0;
        // for (int dx = -1; dx <= 1; dx++)
        // {
        //     for (int dy = -1; dy <= 1; dy++)
        //     {
        //         int2 newPos = new(agentData.unitOgPos.x + dx, agentData.unitOgPos.y + dy);
        //         if (newPos.x < 0 || newPos.x >= ogWidht || newPos.y < 0 || newPos.y >= ogHeight) continue;
        //         int newIndex = newPos.x * ogHeight + newPos.y;

        //         foreach (var other in unitDataList[newIndex])
        //         {
        //             if (other == agentData) continue;

        //             float2 diff = other.position - agentData.position;
        //             if (math.all(diff == float2.zero))
        //                 diff = new float2(rand.NextFloat(-1f, 1f), rand.NextFloat(-1f, 1f));
        //             if (math.lengthsq(diff) < math.pow(agentData.unitRadius + other.unitRadius, 2))
        //             {
        //                 offsetSum += (agentData.unitRadius + other.unitRadius) * math.normalize(diff) - diff;
        //                 count++;
        //             }
        //         }
        //     }
        // }
        // if (count > 0)
        // {
        //     bool offsetSumMask = math.lengthsq(agentData.velocity) > math.lengthsq(offsetSum);
        //     agentData.velocity -= offsetSumMask ? math.length(agentData.velocity) * math.normalize(offsetSum) : offsetSum;
        //     agentData.velocity = UsefulUtils.ClampMagnitude(agentData.velocity, curMaxSpeed);
        // }

        // // KenimaticVelocityCorrection()
        // int ogIndex = agentData.unitOgPos.x * ogHeight + agentData.unitOgPos.y;
        // foreach (var obstacle in obstacleList[ogIndex])
        // {
        //     switch (obstacle.type)
        //     {
        //         case ObstacleType.Circle:
        //             if (UsefulUtils.HasCollideWithCircleObstacle(obstacle.circle, agentData.position, agentData.unitRadius, out var negImpactDir))
        //                 agentData.velocity = UsefulUtils.ProjectOnLine(agentData.velocity, negImpactDir);
        //             break;
        //         case ObstacleType.Rectangle:
        //             if (UsefulUtils.HasCollideWithRectObstacle(obstacle.rect, agentData.position, agentData.unitRadius, out negImpactDir))
        //                 agentData.velocity = UsefulUtils.ProjectOnLine(agentData.velocity, negImpactDir);
        //             break;
        //     }
        // }

        // Damping
        // agentData.velocity *= math.exp(-8f * deltaTime);
        agentData.position += deltaTime * agentData.velocity;

        // // KenimaticPositionCorrection()
        // agentData.position += agentData.velocity * deltaTime;
        // foreach (var obstacle in obstacleList[ogIndex])
        // {
        //     switch (obstacle.type)
        //     {
        //         case ObstacleType.Circle:
        //             agentData.position = UsefulUtils.IfIntersectWithCircleObstacle(obstacle.circle, agentData.position, agentData.unitRadius);
        //             break;
        //         case ObstacleType.Rectangle:
        //             agentData.position = UsefulUtils.IfIntersectWithRectObstacle(obstacle.rect, agentData.position, agentData.unitRadius);
        //             break;
        //     }
        // }

        agentDatas[index] = agentData;
    }
}
