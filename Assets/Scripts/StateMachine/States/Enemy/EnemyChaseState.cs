using UnityEngine;

public class EnemyChaseState: EnemyBaseState
{
    public EnemyChaseState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    
    public override void Enter()
    {
        base.Enter();
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
    }
}
