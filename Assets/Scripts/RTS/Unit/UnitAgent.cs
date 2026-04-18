using Unity.Mathematics;
using UnityEngine;

public class UnitAgent : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float unitRadius;
    [SerializeField] private SkinnedMeshRenderer smr;
    [SerializeField] private float clipLength;
    [SerializeField] private int clipFrame;
    [SerializeField][Range(0, 20)] private float speed;

    [HideInInspector] public int id;

    private void ChangeID(UnitRemoveEvent evt)
    {
        if (id != evt.oldID) return;
        id = evt.newID;
    }

    Quaternion rot = Quaternion.identity;
    float2 lastPos;
    float lastPosUpdateTimer;

    private void UpdateTransform()
    {
        UnitAgentData data = UnitRegister.instance.unitRegistry[id];
        float2 pos = data.position;
        float2 posToLast = pos - lastPos;
        if (!UsefulUtils.Approximately(posToLast, float2.zero))
        {
            if (lastPosUpdateTimer > 0.1f)
            {
                // 0.8f 只是一个参数，控制判断阈值
                if (math.lengthsq(posToLast) > 0.8f * 0.1f * data.curMaxSpeed) lastPos = pos;
                lastPosUpdateTimer = 0;
            }
            else
            {
                lastPosUpdateTimer += Time.deltaTime;
            }

            Quaternion desiredRot = Quaternion.LookRotation(new(posToLast.x, 0, posToLast.y), Vector3.up);
            rot = Quaternion.Slerp(tr.rotation, desiredRot, 4f * Time.deltaTime);
        }

        tr.SetPositionAndRotation(new(pos.x, tr.position.y, pos.y), rot);
    }

    private bool isMoving;
    private bool isAttacking;

    private void UpdateAnimationState()
    {
        UnitAgentData data = UnitRegister.instance.unitRegistry[id];

        if (math.lengthsq(data.velocity) < 1) isMoving = false;
        else isMoving = true;
    }

    private static readonly int frame1Prop = Shader.PropertyToID("_Frame1");
    private static readonly int frame2Prop = Shader.PropertyToID("_Frame2");
    private static readonly int tProp = Shader.PropertyToID("_T");
    private int curStateFrame, lerpFrame;
    private int frameOffset;
    private bool shouldLerp;

    private void OnStateStart(AnimationState state, bool usingJudge)
    {
        if (usingJudge && stateChangeTimer < 0.3f * clipLength) return;
        if (usingJudge) animStateMachine.curState = state;

        lerpFrame = curStateFrame;
        animationTimer = 0;
        stateChangeTimer = 0;
        shouldLerp = true;

        frameOffset = state switch
        {
            AnimationState.Idle => 0,
            AnimationState.Walk => 90,
            AnimationState.Attack => 180,
            _ => 0
        };
    }

    private void OnStateUpdate(AnimationState state)
    {
        curStateFrame = (int)math.round(math.frac(animationTimer * speed / clipLength) * clipFrame + frameOffset);
        shouldLerp = shouldLerp && animationTimer < 0.25f * clipLength;
        float t = shouldLerp ? 1 - math.saturate(animationTimer * speed / (0.25f * clipLength)) : 0;

        mpb.SetFloat(frame1Prop, curStateFrame);
        mpb.SetFloat(frame2Prop, lerpFrame);
        mpb.SetFloat(tProp, t);

        smr.SetPropertyBlock(mpb);
    }

    private Transform tr;
    private MaterialPropertyBlock mpb;
    private AnimationStateMachine animStateMachine;

    private void Awake()
    {
        tr = transform;
        float2 pos = new(tr.position.x, tr.position.z);
        lastPos = pos;
        UnitAgentData data = new(unitRadius, moveSpeed, pos);
        id = UnitRegister.instance.Register(data);

        mpb = new();
        animStateMachine = new(AnimationState.Idle, OnStateStart, OnStateUpdate);

        animStateMachine.AddTransition(AnimationState.Idle, AnimationState.Walk, () => isMoving);
        animStateMachine.AddTransition(AnimationState.Walk, AnimationState.Idle, () => !isMoving);
        animStateMachine.AddTransition(AnimationState.Idle, AnimationState.Attack, () => isAttacking);
        animStateMachine.AddTransition(AnimationState.Attack, AnimationState.Idle, () => !isAttacking);
        animStateMachine.AddTransition(AnimationState.Attack, AnimationState.Walk, () => isMoving);
        animStateMachine.AddTransition(AnimationState.Walk, AnimationState.Attack, () => isAttacking);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<UnitRemoveEvent>(ChangeID);
    }

    private float animationTimer;
    private float stateChangeTimer;

    private void Update()
    {
        animationTimer += Time.deltaTime;
        stateChangeTimer += Time.deltaTime;

        UpdateTransform();
        UpdateAnimationState();
        animStateMachine.Update();
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<UnitRemoveEvent>(ChangeID);

        if (UnitRegister.instance != null)
            UnitRegister.instance.Unregister(id);
    }
}
