using System;
using Unity.Mathematics;

public enum ObstacleType
{
    Circle,
    Rectangle
}

public struct Obstacles : IEquatable<Obstacles>
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
    public static bool operator ==(Obstacles a, Obstacles b) => a.type == b.type && a.circle == b.circle && a.rect == b.rect;
    public static bool operator !=(Obstacles a, Obstacles b) => !(a == b);
    public override readonly bool Equals(object obj) => obj is Obstacles other && this == other;
    public override readonly int GetHashCode() => base.GetHashCode();
    public readonly bool Equals(Obstacles other) => this == other;
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
    public static bool operator ==(Circle a, Circle b) => math.lengthsq(a.center - b.center) < 1e-12f && a.radius == b.radius;
    public static bool operator !=(Circle a, Circle b) => !(a == b);
    public override readonly bool Equals(object obj) => obj is Circle other && this == other;
    public override readonly int GetHashCode() => base.GetHashCode();
}

public struct Rectangle
{
    public float2 center;
    public float2 size;
    public float2 right;
    public float2 up;

    public Rectangle(float2 center, float2 size, float2 right, float2 up)
    {
        this.center = center;
        this.size = size;
        this.right = right;
        this.up = up;
    }
    public static bool operator ==(Rectangle a, Rectangle b) => math.lengthsq(a.center - b.center) < 1e-12f && math.lengthsq(a.size - b.size) < 1e-12f && math.lengthsq(a.right - b.right) < 1e-12f && math.lengthsq(a.up - b.up) < 1e-12f;
    public static bool operator !=(Rectangle a, Rectangle b) => !(a == b);
    public override readonly bool Equals(object obj) => obj is Rectangle other && this == other;
    public override readonly int GetHashCode() => base.GetHashCode();
}
