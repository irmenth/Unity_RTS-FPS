using Unity.Mathematics;
using UnityEngine;

public class UnitAgent : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float unitRadius;

    [HideInInspector] public int id;

    private void ChangeID(UnitRemoveEvent evt)
    {
        if (id != evt.oldID) return;
        id = evt.newID;
    }

    private void Awake()
    {
        float2 pos = new(transform.position.x, transform.position.z);
        UnitAgentData data = new(unitRadius, moveSpeed, pos);
        id = UnitRegister.instance.Register(data);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<UnitRemoveEvent>(ChangeID);
    }

    private void Update()
    {
        float2 pos = UnitRegister.instance.unitRegistry[id].position;
        // float2 veloDir = math.normalizesafe(UnitRegister.instance.unitRegistry[id].velocity);
        // Quaternion desiredRot = Quaternion.LookRotation(new(veloDir.x, 0, veloDir.y), Vector3.up);
        // Quaternion rot = Quaternion.Slerp(transform.rotation, desiredRot, 5f * Time.deltaTime);
        transform.SetPositionAndRotation(new(pos.x, transform.position.y, pos.y), Quaternion.identity);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<UnitRemoveEvent>(ChangeID);

        if (UnitRegister.instance != null)
            UnitRegister.instance.Unregister(id);
    }
}
