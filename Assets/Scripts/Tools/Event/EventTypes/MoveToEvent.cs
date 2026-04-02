using Unity.Mathematics;

public struct MoveToEvent
{
	public float2 destination;

	public MoveToEvent(float2 destination)
	{
		this.destination = destination;
	}
}
