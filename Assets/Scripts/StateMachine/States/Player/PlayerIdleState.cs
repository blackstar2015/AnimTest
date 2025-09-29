using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        stateMachine.Controller.JumpAction += Jump;
        stateMachine.Controller.DodgeAction += Dodge;
        stateMachine.Controller.BlockAction += Block;
        stateMachine.Controller.AttackAction += Attack;
    }

    public override void Exit()
    {
        stateMachine.Controller.JumpAction -= Jump;
        stateMachine.Controller.DodgeAction -= Dodge;
        stateMachine.Controller.BlockAction -= Block;
        stateMachine.Controller.AttackAction -= Attack;
        base .Exit();
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);       
    }


}
