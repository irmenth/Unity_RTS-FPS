using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct UpdateUnitPositionJob : IJobParallelFor
{
    private readonly int2 dgSize, ogSize;
    private readonly float dcRadius, ocRadius;
    private readonly float deltaTime;
    private readonly bool arrived;
    private NativeArray<UnitAgentData> unitReg;
    [ReadOnly] private NativeArray<UnitAgentData> unitRegRO;
    [ReadOnly] private NativeArray<DirectionCell> directionGrid;
    [ReadOnly] private NativeArray<ObstacleData> obstacleReg;
    [ReadOnly] private NativeParallelMultiHashMap<int, int> cellToObstacle;
    [ReadOnly] private NativeParallelMultiHashMap<int, int> cellToUnit;

    public UpdateUnitPositionJob(
        int2 dgSize,
        int2 ogSize,
        float dcRadius,
        float ocRadius,
        float deltaTime,
        bool arrived,
        NativeArray<UnitAgentData> unitReg,
        NativeArray<UnitAgentData> unitRegRO,
        NativeArray<DirectionCell> directionGrid,
        NativeArray<ObstacleData> obstacleReg,
        NativeParallelMultiHashMap<int, int> cellToObstacle,
        NativeParallelMultiHashMap<int, int> cellToUnit
        )
    {
        this.dgSize = dgSize;
        this.ogSize = ogSize;
        this.dcRadius = dcRadius;
        this.ocRadius = ocRadius;
        this.deltaTime = deltaTime;
        this.arrived = arrived;
        this.unitReg = unitReg;
        this.unitRegRO = unitRegRO;
        this.directionGrid = directionGrid;
        this.obstacleReg = obstacleReg;
        this.cellToObstacle = cellToObstacle;
        this.cellToUnit = cellToUnit;
    }

    public void Execute(int index)
    {
        UnitAgentData agentData = unitReg[index];

        Random rand = new((uint)index + 1);
        int steps = (int)math.ceil(agentData.radius / ocRadius);
        // unit 向内检测的步长，超过此步长的内部区域将跳过检测，数值应 >= 2
        int innerSteps = 2;
        int2 dcPos = new(agentData.dgIndex / dgSize.y, agentData.dgIndex % dgSize.y);
        int2 ogPos = new(agentData.ogIndex / ogSize.y, agentData.ogIndex % ogSize.y);
        bool useAlign = UsefulUtils.Approximately(directionGrid[agentData.dgIndex].direction, new float2(-1, -1));

        // FlowFieldVelocity();
        float cost = directionGrid[agentData.dgIndex].cost;
        float curMaxSpeed = math.isinf(cost) ? agentData.speed : agentData.speed / cost;
        if (!arrived)
        {
            float2 dir = float2.zero;
            if (useAlign)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int2 newPos = new(dcPos.x + dx, dcPos.y + dy);
                        if (newPos.x < 0 || newPos.x >= dgSize.x || newPos.y < 0 || newPos.y >= dgSize.y) continue;
                        int newIndex = newPos.x * dgSize.y + newPos.y;

                        float2 newDir = directionGrid[newIndex].direction;
                        if (UsefulUtils.Approximately(newDir, new float2(-1, -1))) continue;

                        dir += newDir;
                    }
                }
                dir = math.normalizesafe(dir);
            }
            else
            {
                dir = directionGrid[agentData.dgIndex].direction;
            }
            agentData.velocity += 4f * curMaxSpeed * deltaTime * dir;
            agentData.velocity = UsefulUtils.ClampMagnitude(agentData.velocity, curMaxSpeed);
        }

        // BoidsVelocityCorrection();
        float2 sepAccSum = float2.zero;
        float2 alignAccSum = float2.zero;
        int count = 0;
        for (int dx = -steps; dx <= steps; dx++)
        {
            for (int dy = -steps; dy <= steps; dy++)
            {
                if (steps >= innerSteps && dx >= -steps + innerSteps && dx <= steps - innerSteps && dy >= -steps + innerSteps && dy <= steps - innerSteps) continue;

                int2 newPos = new(ogPos.x + dx, ogPos.y + dy);
                if (newPos.x < 0 || newPos.x >= ogSize.x || newPos.y < 0 || newPos.y >= ogSize.y) continue;
                int newIndex = newPos.x * ogSize.y + newPos.y;

                if (cellToUnit.TryGetFirstValue(newIndex, out int id, out NativeParallelMultiHashMapIterator<int> it))
                {
                    do
                    {
                        if (id == agentData.id) continue;
                        UnitAgentData data = unitRegRO[id];

                        float2 diff = data.position - agentData.position;
                        float maxDist = agentData.radius + 1.5f * data.radius;
                        if (math.lengthsq(diff) < math.pow(maxDist, 2))
                        {
                            float dist = math.length(diff);
                            float2 sepDir = dist < 1e-6f ? rand.NextFloat2Direction() : diff / dist;
                            float linearFactor = 1f - dist / maxDist;
                            float overLap = math.clamp(400f * curMaxSpeed * (agentData.radius + data.radius - dist), 0, float.PositiveInfinity);
                            float radiusFactor = math.clamp(data.radius / agentData.radius, 0.4f, 4f);
                            float mag = (4f * curMaxSpeed * linearFactor + overLap) * radiusFactor;
                            sepAccSum += mag * sepDir;
                            alignAccSum += useAlign ? mag * math.normalizesafe(data.velocity) : float2.zero;
                            count++;
                        }
                    } while (cellToUnit.TryGetNextValue(out id, ref it));
                }
            }
        }
        if (count > 0)
        {
            sepAccSum = UsefulUtils.ClampMagnitude(sepAccSum, 40f * curMaxSpeed);
            if (!useAlign)
            {
                agentData.velocity += deltaTime * -sepAccSum;
            }
            else
            {
                alignAccSum = UsefulUtils.ClampMagnitude(alignAccSum, 20f * curMaxSpeed);
                agentData.velocity += deltaTime * (alignAccSum - sepAccSum);
            }
            agentData.velocity = UsefulUtils.ClampMagnitude(agentData.velocity, curMaxSpeed);
        }

        // KenimaticVelocityCorrection()
        for (int dx = -steps; dx <= steps; dx++)
        {
            for (int dy = -steps; dy <= steps; dy++)
            {
                if (steps >= innerSteps && dx >= -steps + innerSteps && dx <= steps - innerSteps && dy >= -steps + innerSteps && dy <= steps - innerSteps) continue;

                int2 newPos = new(ogPos.x + dx, ogPos.y + dy);
                if (newPos.x < 0 || newPos.x >= ogSize.x || newPos.y < 0 || newPos.y >= ogSize.y) continue;
                int newIndex = newPos.x * ogSize.y + newPos.y;

                if (cellToObstacle.TryGetFirstValue(newIndex, out int id, out NativeParallelMultiHashMapIterator<int> it))
                {
                    do
                    {
                        ObstacleData data = obstacleReg[id];
                        switch (data.type)
                        {
                            case ObstacleType.Circle:
                                if (UsefulUtils.HasCollideWithCircleObstacle(data.circle, agentData.position, agentData.radius, out float2 negImpactDir))
                                    agentData.velocity = UsefulUtils.ProjectOnLine(agentData.velocity, negImpactDir);
                                break;
                            case ObstacleType.Rectangle:
                                if (UsefulUtils.HasCollideWithRectObstacle(data.rect, agentData.position, agentData.radius, out negImpactDir))
                                    agentData.velocity = UsefulUtils.ProjectOnLine(agentData.velocity, negImpactDir);
                                break;
                        }
                    } while (cellToObstacle.TryGetNextValue(out id, ref it));
                }
            }
        }

        // Damping()
        if (math.lengthsq(agentData.velocity) > 1e-12f)
        {
            agentData.velocity *= math.exp(-8f * deltaTime);
            agentData.position += deltaTime * agentData.velocity;
        }
        else
        {
            agentData.velocity = float2.zero;
        }

        // KenimaticPositionCorrection()
        for (int dx = -steps; dx <= steps; dx++)
        {
            for (int dy = -steps; dy <= steps; dy++)
            {
                if (steps >= innerSteps && dx >= -steps + innerSteps && dx <= steps - innerSteps && dy >= -steps + innerSteps && dy <= steps - innerSteps) continue;

                int2 newPos = new(ogPos.x + dx, ogPos.y + dy);
                if (newPos.x < 0 || newPos.x >= ogSize.x || newPos.y < 0 || newPos.y >= ogSize.y) continue;
                int newIndex = newPos.x * ogSize.y + newPos.y;

                if (cellToObstacle.TryGetFirstValue(newIndex, out int id, out NativeParallelMultiHashMapIterator<int> it))
                {
                    do
                    {
                        ObstacleData data = obstacleReg[id];
                        switch (data.type)
                        {
                            case ObstacleType.Circle:
                                agentData.position = UsefulUtils.IfIntersectWithCircleObstacle(data.circle, agentData.position, agentData.radius);
                                break;
                            case ObstacleType.Rectangle:
                                agentData.position = UsefulUtils.IfIntersectWithRectObstacle(data.rect, agentData.position, agentData.radius);
                                break;
                        }
                    } while (cellToObstacle.TryGetNextValue(out id, ref it));
                }
            }
        }

        unitReg[index] = agentData;
    }
}
