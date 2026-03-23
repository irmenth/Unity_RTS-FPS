using UnityEngine;

public struct Circle
{
    public Vector2 center;
    public float radius;

    public Circle(Vector2 center, float radius)
    {
        this.center = center;
        this.radius = radius;
    }
}

public struct Rectangle
{
    public Vector2 center;
    public Vector2 size;
    public float angle;
    public Vector2[] verteices;

    public Rectangle(Vector2 center, Vector2 size, float angle)
    {
        this.center = center;
        this.size = size;
        this.angle = angle;

        verteices = new Vector2[4];
        verteices[0] = -0.5f * size.x * Mathf.Cos(angle) * Vector2.right + 0.5f * size.y * Mathf.Sin(angle) * Vector2.up + center;
        verteices[1] = 0.5f * size.x * Mathf.Cos(angle) * Vector2.right + 0.5f * size.y * Mathf.Sin(angle) * Vector2.up + center;
        verteices[2] = 0.5f * size.x * Mathf.Cos(angle) * Vector2.right - 0.5f * size.y * Mathf.Sin(angle) * Vector2.up + center;
        verteices[3] = -0.5f * size.x * Mathf.Cos(angle) * Vector2.right - 0.5f * size.y * Mathf.Sin(angle) * Vector2.up + center;
    }
}
