using Unity.Mathematics;
using UnityEngine;

public class ObstacleCollider : MonoBehaviour
{
    [SerializeField] private ObstacleType obstacleType;
    [SerializeField] private float circleRadius;
    [SerializeField] private Vector2 rectBaseSize = Vector2.one;

    public Obstacles obstacle;

    private void Awake()
    {
        var pos = new float2(transform.position.x, transform.position.z);
        obstacle = new Obstacles(obstacleType, new Circle(pos, circleRadius), new Rectangle(pos, rectBaseSize));
    }
}
