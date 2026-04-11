using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMove : MonoBehaviour
{
    [SerializeField][Range(0, 100)] private float moveSpeed = 5f;
    [SerializeField] private Vector2 mapRightTop;
    [SerializeField] private Vector2 mapLeftBottom;

    private bool shouldMove;
    private Vector2 mouseStartPos;

    private void MoveCamera()
    {
        Vector2 mouseCurPos = Pointer.current.position.ReadValue();
        Vector2 delta = mouseCurPos - mouseStartPos;
        Vector3 targetPos = tr.position + moveSpeed * Time.deltaTime * new Vector3(-delta.x, 0, -delta.y);
        targetPos.x = Mathf.Clamp(targetPos.x, mapLeftBottom.x, mapRightTop.x);
        targetPos.z = Mathf.Clamp(targetPos.z, mapLeftBottom.y, mapRightTop.y);
        tr.position = targetPos;
        mouseStartPos = mouseCurPos;
    }

    private void ShouldMove(InputAction.CallbackContext ctx)
    {
        mouseStartPos = Pointer.current.position.ReadValue();
        shouldMove = true;
    }

    private void ShouldNotMove(InputAction.CallbackContext ctx) => shouldMove = false;

    private Transform tr;

    private void Awake()
    {
        tr = transform;
    }

    private void OnEnable()
    {
        InputActionsManager.RTSCameraMove.performed += ShouldMove;
        InputActionsManager.RTSCameraMove.canceled += ShouldNotMove;
    }

    private void Update()
    {
        if (!shouldMove) return;
        MoveCamera();
    }

    private void OnDisable()
    {
        InputActionsManager.RTSCameraMove.performed -= ShouldMove;
        InputActionsManager.RTSCameraMove.canceled -= ShouldNotMove;
    }
}
