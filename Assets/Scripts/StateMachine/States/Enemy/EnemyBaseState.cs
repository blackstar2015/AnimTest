using UnityEngine;

public abstract class EnemyBaseState: State
{
    public EnemyStateMachine stateMachine;
    protected bool _shouldFade;
    protected float _crossfadeDuration = .1f;

    public EnemyBaseState(EnemyStateMachine stateMachine, bool shouldFade = true)
    {
        this.stateMachine = stateMachine;
        _shouldFade = shouldFade;
    }
    
    public override void Enter()
    {
        if (stateMachine.DebugStateTransitions) Debug.Log("Entering " + stateMachine.CurrentState);
    }
    public override void Exit()
    {
        if (stateMachine.DebugStateTransitions) Debug.Log("Exiting " + stateMachine.CurrentState);
    }
    public override void Tick(float deltaTime)
    {
        if (stateMachine.DebugStateTransitions) Debug.Log("Current State " + stateMachine.CurrentState);
    }

}
