using Unity.Mathematics;
using UnityEngine;

public enum ObstacleType
{
    Circle,
    Rectangle
}

public struct Obstacles
{
    public ObstacleType type;
    public Circle circle;
    public Rectangle rect;

    public Obstacles(ObstacleType type, Circle circle, Rectangle rect)
    {
        this.type = type;
        this.circle = circle;
        this.rect = rect;
    }
}

public struct Circle
{
    public float2 center;
    public float radius;

    public Circle(float2 center, float radius)
    {
        this.center = center;
        this.radius = radius;
    }
}

public struct Rectangle
{
    public float2 center;
    public float2 baseSize;

    public Rectangle(float2 center, float2 baseSize)
    {
        this.center = center;
        this.baseSize = baseSize;
    }
}
