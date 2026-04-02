using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class GridController : MonoBehaviour
{
    [Header("In Game")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Vector2Int directionGridSize;
    [SerializeField] private float directionCellRadius = 0.2f;
    [SerializeField] private Vector2Int obstacleGridSize;
    [SerializeField] private float obstacleCellRadius = 0.5f;
    [SerializeField] private LayerMask costLayerMask;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask impassibleLayerMask;
    [SerializeField] private LayerMask roughLayerMask;
    [Header("Debug Gizmos")]
    [SerializeField] private GameObject dirIndictorPrefab;
    [SerializeField] private Material upArrow;
    [SerializeField] private Material downArrow;
    [SerializeField] private Material leftArrow;
    [SerializeField] private Material rightArrow;
    [SerializeField] private Material upLeftArrow;
    [SerializeField] private Material upRightArrow;
    [SerializeField] private Material downLeftArrow;
    [SerializeField] private Material downRightArrow;
    [SerializeField] private Material cross;
    [SerializeField] private Material flag;

    public FlowField CurFlowField { get; private set; }

    // Gizmos for debug in editor
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (CurFlowField == null) return;

        // Draw direction grid
        Gizmos.color = Color.yellow;
        for (int x = 0; x < directionGridSize.x; x++)
        {
            for (int y = 0; y < directionGridSize.y; y++)
            {
                Gizmos.DrawWireCube(CurFlowField.DirGrid[x, y].GetWorldPos(), directionCellRadius * 2f * Vector3.one);
            }
        }

        // // // Draw obstacle grid
        // Gizmos.color = Color.yellow;
        // for (int x = 0; x < obstacleGridSize.x; x++)
        // {
        //     for (int y = 0; y < obstacleGridSize.y; y++)
        //     {
        //         Gizmos.DrawWireCube(CurFlowField.ObstacleGrid[x, y].GetWorldPos(), obstacleCellRadius * 2f * Vector3.one);
        //     }
        // }

        // // Draw Cost Field
        // for (int x = 0; x < directionGridSize.x; x++)
        // {
        //     for (int y = 0; y < directionGridSize.y; y++)
        //     {
        //         var pos = CurFlowField.DirGrid[x, y].GetWorldPos() + directionCellRadius * Vector3.left;
        //         UnityEditor.Handles.Label(pos, CurFlowField.DirGrid[x, y].cost.ToString("F1"));
        //     }
        // }

        // // Draw Heat Map
        // for (int x = 0; x < directionGridSize.x; x++)
        // {
        //     for (int y = 0; y < directionGridSize.y; y++)
        //     {
        //         var pos = CurFlowField.DirGrid[x, y].GetWorldPos() + directionCellRadius * Vector3.left;
        //         UnityEditor.Handles.Label(pos, CurFlowField.DirGrid[x, y].heat.ToString("F1"));
        //     }
        // }

        // // Draw Obstacle Count
        // for (int x = 0; x < obstacleGridSize.x; x++)
        // {
        //     for (int y = 0; y < obstacleGridSize.y; y++)
        //     {
        //         var pos = CurFlowField.ObstacleGrid[x, y].GetWorldPos() + obstacleCellRadius * Vector3.left;
        //         var count = CurFlowField.ObstacleGrid[x, y].obstacleList.Count;
        //         UnityEditor.Handles.Label(pos, count.ToString());
        //     }
        // }

        // Draw Flow Field
        for (int x = 0; x < directionGridSize.x; x++)
        {
            for (int y = 0; y < directionGridSize.y; y++)
            {
                var dir = CurFlowField.DirGrid[x, y].direction;
                var pos = CurFlowField.DirGrid[x, y].GetWorldPos() + 10f * Vector3.up;

                Material dirIndictorMat = null;
                if (UsefulUtils.Approximately(dir, new float2(-1, -1)))
                    dirIndictorMat = cross;
                else if (UsefulUtils.Approximately(dir, new float2(0, 1)))
                    dirIndictorMat = upArrow;
                else if (UsefulUtils.Approximately(dir, new float2(0, -1)))
                    dirIndictorMat = downArrow;
                else if (UsefulUtils.Approximately(dir, new float2(-1, 0)))
                    dirIndictorMat = leftArrow;
                else if (UsefulUtils.Approximately(dir, new float2(1, 0)))
                    dirIndictorMat = rightArrow;
                else if (UsefulUtils.Approximately(dir, math.normalize(new float2(1, 1))))
                    dirIndictorMat = upRightArrow;
                else if (UsefulUtils.Approximately(dir, math.normalize(new float2(-1, 1))))
                    dirIndictorMat = upLeftArrow;
                else if (UsefulUtils.Approximately(dir, math.normalize(new float2(1, -1))))
                    dirIndictorMat = downRightArrow;
                else if (UsefulUtils.Approximately(dir, math.normalize(new float2(-1, -1))))
                    dirIndictorMat = downLeftArrow;
                else if (UsefulUtils.Approximately(dir, new float2(0, 0)))
                    dirIndictorMat = flag;

                if (dirIndictorMat != null && dirIndicatorMeshRenderers[x, y] != null)
                    dirIndicatorMeshRenderers[x, y].sharedMaterial = dirIndictorMat;
            }
        }
#endif
    }

    private void SetDestination(InputAction.CallbackContext ctx)
    {
#if UNITY_EDITOR
        Stopwatch sw = new();
        sw.Start();
#endif

        var mousePos = Pointer.current.position.ReadValue();
        var ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out var hit, 1000f, groundLayerMask))
        {
            float2 hitPoint = new(hit.point.x, hit.point.z);
            var mouseGridPos = CurFlowField.WorldToDirGridPos(hitPoint);
            if (math.all(mouseGridPos == new int2(-1, -1))) return;

            CurFlowField.GenerateHeatMapBurst(mouseGridPos);
            CurFlowField.GenerateFlowFieldBurst();

            EventBus.Publish(new MoveToEvent(hitPoint));
        }

#if UNITY_EDITOR
        sw.Stop();
        Debug.Log($"[GridController] heat map & flow field generation: {sw.ElapsedMilliseconds}ms");
#endif
    }

    private int unitCount;

    private void GenerateUnit(InputAction.CallbackContext ctx)
    {
        var mousePos = Pointer.current.position.ReadValue();
        var ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out var hit, 1000f, groundLayerMask))
        {
            Instantiate(unitPrefab, hit.point, Quaternion.identity);
            unitCount++;
            Debug.Log($"[GridController] unit count: {unitCount}");
        }
    }

#if UNITY_EDITOR
    private MeshRenderer[,] dirIndicatorMeshRenderers;
#endif
    private int impassibleLayer, roughLayer;

    private void Awake()
    {
        impassibleLayer = UsefulUtils.GetLayer(impassibleLayerMask);
        roughLayer = UsefulUtils.GetLayer(roughLayerMask);

        InputActionsManager.RTSSetDestination.started += SetDestination;
        InputActionsManager.RTSGenerateUnit.started += GenerateUnit;
    }

    private void Start()
    {
        CurFlowField = new FlowField(directionGridSize.x, directionGridSize.y, directionCellRadius, obstacleGridSize.x, obstacleGridSize.y, obstacleCellRadius);
        CurFlowField.GenerateGrid();
        CurFlowField.GenerateCostField(costLayerMask, impassibleLayer, roughLayer);
        CurFlowField.GenerateObstacleMap(impassibleLayer);

#if UNITY_EDITOR
        dirIndicatorMeshRenderers = new MeshRenderer[directionGridSize.x, directionGridSize.y];

        for (int x = 0; x < directionGridSize.x; x++)
        {
            for (int y = 0; y < directionGridSize.y; y++)
            {
                var pos = CurFlowField.DirGrid[x, y].GetWorldPos() + 10 * Vector3.up;
                dirIndicatorMeshRenderers[x, y] = Instantiate(dirIndictorPrefab, pos, Quaternion.Euler(90, 0, 0)).GetComponent<MeshRenderer>();
            }
        }
#endif
    }

    private void OnDestroy()
    {
        InputActionsManager.RTSSetDestination.started -= SetDestination;
        InputActionsManager.RTSGenerateUnit.started -= GenerateUnit;
    }
}
