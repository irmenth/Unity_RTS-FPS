using Unity.Mathematics;
using UnityEngine;

public class ObstacleCollider : MonoBehaviour
{
    [SerializeField] private ObstacleType obstacleType;
    [SerializeField] private Transform obstacleTransform;
    [SerializeField] private float circleRadius;
    [SerializeField] private float2 rectSize = new(1, 1);

    public Obstacles obstacle;

    private void Awake()
    {
        float2 pos = new(transform.position.x, transform.position.z);
        float2 right = new(transform.right.x, transform.right.z);
        float2 up = new(transform.forward.x, transform.forward.z);
        obstacle = new Obstacles(obstacleType, new Circle(pos, circleRadius), new Rectangle(pos, rectSize, right, up));
    }
}
