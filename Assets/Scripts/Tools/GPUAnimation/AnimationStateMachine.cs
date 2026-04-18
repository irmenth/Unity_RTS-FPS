using System;
using System.Collections.Generic;

public enum AnimationState
{
    Idle,
    Walk,
    Attack,
}

public class AnimationStateMachine
{
    public AnimationState curState;
    private readonly Dictionary<(AnimationState from, AnimationState to), Func<bool>> transitionTable = new();
    private readonly Action<AnimationState, bool> onStateStart;
    private readonly Action<AnimationState> onStateUpdate;

    public AnimationStateMachine(AnimationState curState, Action<AnimationState, bool> onStateStart, Action<AnimationState> onStateUpdate)
    {
        this.curState = curState;
        this.onStateStart = onStateStart;
        this.onStateUpdate = onStateUpdate;

        onStateStart?.Invoke(curState, false);
    }

    public void AddTransition(AnimationState from, AnimationState to, Func<bool> condition)
    {
        if (!transitionTable.ContainsKey((from, to)))
            transitionTable.Add((from, to), condition);
    }

    public void Update()
    {
        foreach (var transition in transitionTable)
        {
            if (transition.Key.from == curState && transition.Value())
            {
                onStateStart?.Invoke(transition.Key.to, true);
                break;
            }
        }
        onStateUpdate?.Invoke(curState);
    }
}
