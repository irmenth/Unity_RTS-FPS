using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private float cellRadius = 0.5f;
    [SerializeField] private LayerMask costLayerMask;
    [SerializeField] private LayerMask impassibleLayerMask;
    [SerializeField] private LayerMask roughLayerMask;

    public FlowField CurFlowField { get; private set; }

    // Gizmos for debug in editor
    private void OnDrawGizmos()
    {
        if (CurFlowField == null) return;

        // Draw grid
        Gizmos.color = Color.yellow;
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Gizmos.DrawWireCube(CurFlowField.Grid[x, y].WorldPos, cellRadius * 2f * Vector3.one);
            }
        }

        // Draw Cost Field
#if UNITY_EDITOR
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                UnityEditor.Handles.Label(CurFlowField.Grid[x, y].WorldPos, CurFlowField.Grid[x, y].Cost.ToString());
            }
        }
#endif
    }

    private int impassibleLayer, roughLayer;

    private void Awake()
    {
        impassibleLayer = UsefulUtils.GetLayer(impassibleLayerMask);
        roughLayer = UsefulUtils.GetLayer(roughLayerMask);
    }

    private void Start()
    {
        CurFlowField = new FlowField(gridSize.x, gridSize.y, cellRadius);
        CurFlowField.GenerateGrid();
        CurFlowField.GenerateCostField(costLayerMask, impassibleLayer, roughLayer);
    }
}
