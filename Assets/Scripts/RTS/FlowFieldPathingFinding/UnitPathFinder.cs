using UnityEngine;

public class UnitPathFinder : MonoBehaviour
{
    [SerializeField] private GridController gridCC;
    [SerializeField] private float unitRadius;
    [SerializeField] private float moveSpeed;

    private Vector2 velocity;

    private void SteeringGetVelocity(FlowField flowField, Vector2Int unitGridPos)
    {
    }

    private void MoveByFlowField()
    {
        Vector2Int unitGridPos = gridCC.CurFlowField.WorldToGridPos(transform.position);
        if (unitGridPos == new Vector2Int(-1, -1)) return;

        SteeringGetVelocity(gridCC.CurFlowField, unitGridPos);

        transform.position = Time.deltaTime * UsefulUtils.V2ToV3(velocity) + transform.position;
    }

    private void Update()
    {
        MoveByFlowField();
    }
}
