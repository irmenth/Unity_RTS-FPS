using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class GridController : MonoBehaviour
{
    [Header("Game Logic")]
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private float cellRadius = 0.5f;
    [SerializeField] private LayerMask costLayerMask;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask impassibleLayerMask;
    [SerializeField] private LayerMask roughLayerMask;
    [Header("Debug Gizmos")]
    [SerializeField] private GameObject upArrow;
    [SerializeField] private GameObject downArrow;
    [SerializeField] private GameObject leftArrow;
    [SerializeField] private GameObject rightArrow;
    [SerializeField] private GameObject upLeftArrow;
    [SerializeField] private GameObject upRightArrow;
    [SerializeField] private GameObject downLeftArrow;
    [SerializeField] private GameObject downRightArrow;
    [SerializeField] private GameObject cross;
    [SerializeField] private GameObject flag;

    public FlowField CurFlowField { get; private set; }

    // Gizmos for debug in editor
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
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

        // // Draw Cost Field
        // for (int x = 0; x < gridSize.x; x++)
        // {
        //     for (int y = 0; y < gridSize.y; y++)
        //     {
        //         var pos = CurFlowField.Grid[x, y].WorldPos + cellRadius * Vector3.left;
        //         UnityEditor.Handles.Label(pos, CurFlowField.Grid[x, y].cost.ToString());
        //     }
        // }

        // // Draw Heat Map
        // for (int x = 0; x < gridSize.x; x++)
        // {
        //     for (int y = 0; y < gridSize.y; y++)
        //     {
        //         var pos = CurFlowField.Grid[x, y].WorldPos + cellRadius * Vector3.left;
        //         UnityEditor.Handles.Label(pos, CurFlowField.Grid[x, y].heat.ToString("F1"));
        //     }
        // }

        // Draw Flow Field
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                var dir = CurFlowField.Grid[x, y].direction;
                var pos = CurFlowField.Grid[x, y].WorldPos + 10f * Vector3.up;

                GameObject texPrefab = null;
                if (dir == new Vector2(-1, -1))
                    texPrefab = cross;
                else if (dir == Vector2.up)
                    texPrefab = upArrow;
                else if (dir == Vector2.down)
                    texPrefab = downArrow;
                else if (dir == Vector2.left)
                    texPrefab = leftArrow;
                else if (dir == Vector2.right)
                    texPrefab = rightArrow;
                else if (dir == new Vector2(0.71f, 0.71f))
                    texPrefab = upRightArrow;
                else if (dir == new Vector2(-0.71f, 0.71f))
                    texPrefab = upLeftArrow;
                else if (dir == new Vector2(0.71f, -0.71f))
                    texPrefab = downRightArrow;
                else if (dir == new Vector2(-0.71f, -0.71f))
                    texPrefab = downLeftArrow;
                else if (dir == Vector2.zero)
                    texPrefab = flag;

                if (texPrefab != null)
                    Instantiate(texPrefab, pos, Quaternion.Euler(90, 0, 0));
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
            CurFlowField.GenerateHeatMap(hit.point);
            CurFlowField.GenerateFlowField();
        }

#if UNITY_EDITOR
        sw.Stop();
        Debug.Log($"SetDestination: {sw.ElapsedMilliseconds}ms");
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

        InputActionsManager.RTSSetDestination.started += SetDestination;
    }

    private void OnDestroy()
    {
        InputActionsManager.RTSSetDestination.started -= SetDestination;
    }
}
