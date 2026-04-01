using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct UnitAgentJob : IJobParallelFor
{
    private readonly int dgWidht, dgHeight, ogWidht, ogHeight;
    private readonly float deltaTime;
    [ReadOnly] private NativeArray<UnitAgentData> agentDatas;
    private NativeArray<float2> velocities;
    [ReadOnly] private NativeArray<float2> directions;
    [ReadOnly] private NativeArray<float> costMap;
    [ReadOnly] private NativeArray<NativeList<UnitAgentData>> unitDataList;
    [ReadOnly] private NativeArray<NativeList<Obstacles>> obstacleList;

    public void Execute(int index)
    {
        // FlowFieldVelocity();
        UnitAgentData agentData = agentDatas[index];
        float cost = costMap[index];
        float curMaxSpeed = math.isinf(cost) ? agentData.moveSpeed : agentData.moveSpeed / cost;
        float2 dir = directions[index];
        if (math.all(dir != new float2(-1, -1)) && math.all(dir != new float2(0, 0)))
        {
            velocities[index] += 4f * curMaxSpeed * deltaTime * dir;
            velocities[index] = UsefulUtils.ClampMagnitude(velocities[index], curMaxSpeed);
        }

        // BoidsVelocityCorrection();
        Random rand = new((uint)index);
        float2 offsetSum = new(0, 0);
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int2 newPos = new(agentData.unitOgPos.x + dx, agentData.unitOgPos.y + dy);
                if (newPos.x < 0 || newPos.x >= ogWidht || newPos.y < 0 || newPos.y >= ogHeight) continue;
                int newIndex = newPos.x * ogHeight + newPos.y;

                foreach (var other in unitDataList[newIndex])
                {
                    if (other == agentData) continue;

                    float2 diff = other.position - agentData.position;
                    if (math.all(diff == float2.zero))
                        diff = new float2(rand.NextFloat(-1f, 1f), rand.NextFloat(-1f, 1f));
                    if (math.lengthsq(diff) < math.pow(agentData.unitRadius + other.unitRadius, 2))
                    {
                        offsetSum += (agentData.unitRadius + other.unitRadius) * math.normalize(diff) - diff;
                        count++;
                    }
                }
            }
        }
        if (count > 0)
        {
            bool offsetSumMask = math.lengthsq(velocities[index]) > math.lengthsq(offsetSum);
            velocities[index] -= math.select(offsetSum, math.length(velocities[index] * math.normalize(offsetSum)), offsetSumMask);
            velocities[index] = UsefulUtils.ClampMagnitude(velocities[index], curMaxSpeed);
        }

        // KenimaticVelocityCorrection()
        foreach (var obstacle in obstacleList[index])
        {
            switch (obstacle.type)
            {
                case ObstacleType.Circle:
                    if (UsefulUtils.HasCollideWithCircleObstacle(obstacle.circle, agentData.position, agentData.unitRadius, out var negImpactDir))
                        velocities[index] = UsefulUtils.ProjectOnLine(velocities[index], negImpactDir);
                    break;
                case ObstacleType.Rectangle:
                    break;
            }
        }

        // Damping
        velocities[index] *= math.exp(-8f * deltaTime);
    }
}
