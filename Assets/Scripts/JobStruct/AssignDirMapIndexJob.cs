using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct AssignDirMapIndexJob : IJob
{
    [ReadOnly] private NativeArray<int> selectedArray;
    private NativeArray<ulong> dirMapIndices;
    private NativeArray<bool> arrived;
    private readonly ulong dirMapID;

    public AssignDirMapIndexJob(
        NativeArray<int> selectedArray,
        NativeArray<ulong> dirMapIndices,
        NativeArray<bool> arrived,
        ulong dirMapID
    )
    {
        this.selectedArray = selectedArray;
        this.dirMapIndices = dirMapIndices;
        this.arrived = arrived;
        this.dirMapID = dirMapID;
    }

    public void Execute()
    {
        foreach (int index in selectedArray)
        {
            dirMapIndices[index] = dirMapID;
            arrived[index] = false;
        }
    }
}
