using UnityEngine;
using UnityEngine.InputSystem;

public class InputActionSystem : MonoBehaviour
{
    public static InputActionSystem instance;

    [SerializeField] private LayerMask groundLayerMask;

    private void GenerateOrangeUnit(InputAction.CallbackContext ctx)
    {
        Vector2 mousePos = Pointer.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayerMask))
        {
            Client.instance.SendInput(new GenerateCommand(UnitType.OrangeSmall, 10, new(hit.point.x, hit.point.z)));
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InputActionsManager.RTSGenerateOrangeUnit.started += GenerateOrangeUnit;
    }

    private void OnDestroy()
    {
        instance = null;

        InputActionsManager.RTSGenerateOrangeUnit.started -= GenerateOrangeUnit;
    }
}
