using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    public PlayerAttackState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
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
        if(!stateMachine.IsAttacking)
        {
            stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
        }
        stateMachine.HandleAttack();
    }
}
